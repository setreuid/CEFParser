using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEFParser.Waiter.Selector
{
    public class CSS : Selector
    {
        private ChromiumWebBrowser webBrowser;

        private String uid;

        private String selector;

        private String frameName = String.Empty;


        /**
         * 생성자
         * CSS Selctor
         * @param plainText
         */
        public CSS(String plainText)
        {
            this.uid = String.Format("CSSSELECTOR{0}{1}", DateTime.Now.Ticks.ToString(), (new Random()).Next(0, 1000));
            this.selector = String.Format("document.querySelectorAll(\"{0}\")", plainText);
        }


        /**
         * 생성자
         * CSS Selctor
         * @param plainText
         */
        public CSS(String plainText, String frameId)
        {
            this.uid = String.Format("CSSSELECTOR{0}{1}", DateTime.Now.Ticks.ToString(), (new Random()).Next(0, 1000));
            this.selector = String.Format("document.querySelectorAll(\"{0}\")", plainText);
            this.frameName = frameId;
        }


        /**
         * 자식 요소 선택
         * @param  text
         * @return CSS
         */
        public CSS Child(String text)
        {
            return new CSS(String.Empty)
                .SetCSSWebBrowser(this.webBrowser)
                .SetSelectorText(String.Format(
                    "{0}.querySelectorAll(\"{1}\")", 
                    this.selector, 
                    text
                ));
        }


        /**
         * 배열상에 있는 특정 인덱스 선택
         * @param  index
         * @todo   메소드 이름을 바꿔야 하지 않을까...
         * @return CSS
         */
        public CSS Child(long index)
        {
            return new CSS(String.Empty)
                .SetCSSWebBrowser(this.webBrowser)
                .SetSelectorText(String.Format(
                    "{0}[{1}]", 
                    this.selector, 
                    index
                ));
        }


        /**
         * 웹 브라우저 주입
         * @param  webBrowser
         * @return CSS
         */
        public CSS SetCSSWebBrowser(ChromiumWebBrowser webBrowser)
        {
            return (CSS)SetWebBrowser(webBrowser);
        }


        /**
         * CSS Selector 문자열 주입
         * @param  selectorText
         * @return CSS
         */
        public CSS SetSelectorText(String selectorText)
        {
            this.selector = selectorText;
            return this;
        }


        /**
         * Document 가져오기
         * frameIndex 설정시 해당 프레임셋의 내부에서 실행하도록 하기 위해
         * @return HtmlDocument
         */
        public async Task<IFrame> GetDocumentAsync()
        {
            if (this.frameName == String.Empty) return this.webBrowser.GetMainFrame();

            foreach (var i in this.webBrowser.GetBrowser().GetFrameIdentifiers())
            {
                var obj = webBrowser.GetBrowser().GetFrame(i);
                var response = await obj.EvaluateScriptAsync("(function() { return window.name })()");

                if ((string) response.Result == this.frameName) return obj;
            }

            return null;
        }


        /**
         * 주입된 CSS 구문으로 DOM 오브젝트 가져오기
         * 
         * 오브젝트가 여러개인경우 배열이 리턴되지 않으므로 Child(index) 메소드를 사용해서 선택해야함.
         * 오브젝트가 하나인경우 해당 오브젝트가 리턴됨.
         * 
         * @exception NullReferenceException 해당 오브젝트가 없을경우
         * @return    DOM OBJECT
         */
        public async Task<dynamic> GetElementsAsync()
        {
            if (GetDocumentAsync() == null) return null;

            dynamic elements;

            try
            {
                elements = (await (await GetDocumentAsync()).EvaluateScriptAsync(
                    String.Format("(function(){{ return {0} }})()", this.selector))).Result;
            }
            catch (NullReferenceException e)
            {
                throw e;
            }

            return elements;
        }


        /**
         * DOM 오브젝트의 이벤트를 강제로 발생시킴
         * @param events
         */
        public async Task TriggerAsync(string events)
        {
            if (GetDocumentAsync() == null) return;
            //InjectCreateStyleSheet();
            //InjectQuerySelectorAll();

            String command = String.Format(@"
                (function() {{
                    var event = document.createEvent('HTMLEvents');
                    event.initEvent(""{1}"", true, true);
                    {0}[0].dispatchEvent(event)
                }})
            ", this.selector, events);

            await (await GetDocumentAsync()).EvaluateScriptAsync(
                        String.Format(@"(function() {{ 
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
                        }})()", command));
        }


        /**
         * DOM 오브젝트의 이벤트를 강제로 발생시킴2
         * @param events
         */
        public async Task TriggerRawAsync(string events)
        {
            if (GetDocumentAsync() == null) return;
            //InjectCreateStyleSheet();
            //InjectQuerySelectorAll();

            String command = String.Format(@"
                (function() {{
                    {0}[0].{1}
                }})
            ", this.selector, events);

            await (await GetDocumentAsync()).EvaluateScriptAsync(
                        String.Format(@"(function() {{ 
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
                        }})()", command));
        }


        /**
         * 해당 셀렉터로 검색된 DOM 객체 갯수를 반환
         * @return 배열 갯수
         */
        public async Task<long> GetCountsAsync()
        {
            if (GetDocumentAsync() == null) return 0;

            String results;

            try
            {
                var response = await (await GetDocumentAsync()).EvaluateScriptAsync(String.Format("(function() {{ return {0}.length }})()", this.selector));
                results = response.Result.ToString();
            }
            catch (NullReferenceException e)
            {
                Trace.Write(e.StackTrace);
                Trace.WriteLine(e.Message);
                throw e;
            }

            long counts;
            if (long.TryParse(results, out counts))
            {
                return counts;
            }
            else
            {
                throw new InvalidCastException(String.Format("Can not cast \"{0}\" to type long.", results));
            }
        }


        /**
         * 해당 셀렉터로 검색된 DOM 객체 갯수를 반환
         * @return 배열 갯수
         */
        public async Task<bool> IsHiddenAsync()
        {
            var ea = GetDocumentAsync();
            if (GetDocumentAsync() == null) return false;

            String results;

            await InjectIsHiddenAsync();
            
            try
            {
                results = (await (await GetDocumentAsync()).EvaluateScriptAsync(
                    String.Format("(function(){{ return isHidden({0}[0]) }})()", this.selector))).Result.ToString();
            }
            catch (NullReferenceException e)
            {
                throw e;
            }
            
            bool isHidden;
            if (bool.TryParse(results, out isHidden))
            {
                return isHidden;
            }
            else
            {
                throw new InvalidCastException(String.Format("Can not cast \"{0}\" to type boolean.", results));
            }
        }


        /**
         * 해당 DOM 객체의 특정 값 가져오기
         * @param  attributeName
         * @return attributeName에 해당하는 값
         */
        public async Task<string> GetAttributeAsync(String attributeName)
        {
            if (GetDocumentAsync() == null) return String.Empty;

            String element;

            try
            {
                element = (await (await GetDocumentAsync()).EvaluateScriptAsync(
                    String.Format("(function(){{ return {0}[0][\"{1}\"] }})()", this.selector, attributeName))).Result.ToString();
            }
            catch (NullReferenceException e)
            {
                throw e;
            }

            return element;
        }


        /**
         * 해당 DOM 객체에 특정 값 주입
         * @param attributeName  Key
         * @param attributeValue Value
         */
        public async Task SetAttributeAsync(String attributeName, String attributeValue)
        {
            await (await GetDocumentAsync()).EvaluateScriptAsync(
                        String.Format("(function(){{ {0}[\"{1}\"] = \"{2}\" }})()",
                            this.selector, attributeName, attributeValue));
        }


        /**
         * 구 IE 브라우저용 핵
         * createStyleSheet 메소드가 없을경우 주입
         */
        private async Task InjectCreateStyleSheetAsync()
        {
            (await GetDocumentAsync()).ExecuteJavaScriptAsync(Utils.IETools.createStyleSheet);
        }


        /**
         * 구 IE 브라우저용 핵
         * querySelectorAll 메소드가 없을경우 주입
         */
        private async Task InjectQuerySelectorAllAsync()
        {
            (await GetDocumentAsync()).ExecuteJavaScriptAsync(Utils.IETools.querySelectorAll2);
        }

        
        /**
         * DOM 객체가 숨어있는지 판단하기 위한 메소드 주입
         */
        private async Task InjectIsHiddenAsync()
        {
            (await GetDocumentAsync()).ExecuteJavaScriptAsync(Utils.IETools.isHidden);
        }


        /**
         * 웹 브라우저 주입
         * @param  webBrowser
         * @return Selector
         */
        public Selector SetWebBrowser(ChromiumWebBrowser webBrowser)
        {
            this.webBrowser = webBrowser;
            return this;
        }
    }
}
