using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEFParser
{
    /**
     * .NET WINFORM 용 IE PARSER
     * 
     * @author 조태호(setreuid@naver.com)
     * @date   2018. 12. 26.
     * 
     * CefSharp 패키지를 사용하여 웹 파싱 처리를 하는데 목적을 가진다.
     * 
     * 이 모듈에서 사용하는 명칭과 설명은 다음과 같다.
     * 1. Parser  : 파싱 클래스 명칭
     * 2. Waiter  : 파싱 작업을 위해서 해당 HTML DOM 객체가 존재하거나
     *              사용 가능 여부를 확인하기 위한 클래스
     * 3. Watcher : Waiter 객체의 행동를 수행하기 위해 주기적으로 체크하는 클래스
     */
    public class Parser
    {
        // 파싱용 브라우저
        private ChromiumWebBrowser webBrowser;

        private Watcher.Watcher watcher;
        
        private Mutex taskMutex;

        
        /**
         * 생성자
         * @param targetBrowser 파싱용 웹 브라우저
         */
        public Parser(ChromiumWebBrowser targetBrowser)
        {
            this.webBrowser = targetBrowser;
            this.taskMutex = new Mutex(false, "Task");
            this.watcher = new Watcher.Watcher(targetBrowser, taskMutex);
        }


        public void Close()
        {
            this.watcher.Close();
        }


        /**
         * HTML DOM 객체 추적 작업 추가
         * @param waiter
         */
        public void AddWaiter(Waiter.Waiter waiter)
        {
            waiter.SetWebBrowser(this.webBrowser);
            this.watcher.Add(waiter);
        }
        

        /**
         * JS 스크립트 실행
         * @param jsString
         */
        public void ExecuteJS(String jsString)
        {
            ExecuteJS(jsString, webBrowser.GetMainFrame());
        }


        /**
         * JS 스크립트 실행
         * @param jsString
         * @param frameIndex
         */
        public async Task ExecuteJSAsync(String jsString, String frameName)
        {
            ExecuteJS(jsString, await GetDocumentByIdAsync(webBrowser.GetMainFrame(), frameName));
        }


        /**
         * JS 스크립트 실행
         * @param jsString
         * @param frameIndex
         */
        public async Task ExecuteJSAsync(String jsString, String[] framesName)
        {
            ExecuteJS(jsString, await GetDocumentAsync(framesName));
        }


        /**
         * Document 가져오기
         * frameIndex 설정시 해당 프레임셋의 내부에서 실행하도록 하기 위해
         * @return HtmlDocument
         */
        public async Task<IFrame> GetDocumentAsync(String[] framesName)
        {
            IFrame frame = this.webBrowser.GetMainFrame();
            foreach (String fName in framesName)
            {
                frame = await GetDocumentByIdAsync(frame, fName);
            }

            return frame;
        }


        /**
         * IFrame 가져오기
         * @param  parentFrame
         * @param  fName
         * @return 검출된 프레임
         */
        public static async Task<IFrame> GetDocumentByNameAsync(IFrame parentFrame, String fName)
        {
            if (fName == String.Empty) return parentFrame;

            foreach (var i in parentFrame.Browser.GetFrameIdentifiers())
            {
                var obj = parentFrame.Browser.GetFrame(i);
                var response = await obj.EvaluateScriptAsync("(function() { return window.name })()");

                if ((string)response.Result == fName) return obj;
            }

            return null;
        }


        /**
         * IFrame 가져오기
         * @param  parentFrame
         * @param  fName
         * @return 검출된 프레임
         */
        public static async Task<IFrame> GetDocumentByIdAsync(IFrame parentFrame, String fName)
        {
            if (fName == String.Empty) return parentFrame;

            foreach (var i in parentFrame.Browser.GetFrameIdentifiers())
            {
                var obj = parentFrame.Browser.GetFrame(i);
                var response = await obj.EvaluateScriptAsync("(function() { return !window.frameElement ? '' : window.frameElement.id || window.frameElement.name })()");

                if ((string)response.Result == fName) return obj;
            }

            return null;
        }


        /**
         * JS 스크립트 실행
         * @param jsString
         * @param document
         */
        public void ExecuteJS(String jsString, CefSharp.IFrame document)
        {
            this.webBrowser.Invoke(new Action(() =>
            {
                String command = String.Format(@"
                    (function() {{
                        window.requestAnimationFrame(function() {{
                            {0}
                        }});
                    }})
                ", jsString);

                String script = String.Format(@"(function() {{ 
                            if (document.readyState != 'loading') {{
                                {0}();
                            }} else if (document.addEventListener) {{
                                document.addEventListener('DOMContentLoaded', {0});
                            }} else {{
                                document.attachEvent('onreadystatechange', function() {{
                                  if (document.readyState != 'loading')
                                    {0}();
                                }});
                            }}
                        }})()", command);

                //JavascriptResponse resutls = await document.EvaluateScriptAsync(script);
                document.ExecuteJavaScriptAsync(script);
            }));
        }
    }
}
