using CefSharp;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEFParser.Waiter.Selector
{
    public interface Selector
    {
        /**
         * 프레임이 있는 홈페이지를 위한 HtmlDocument 반환 메소드
         * @return HtmlDocument
         */
        Task<IFrame> GetDocumentAsync();


        /**
         * DOM 가져오기
         * @exception NullReferenceException
         */
        Task<dynamic> GetElementsAsync();


        /**
         * DOM 배열의 경우 갯수 가져오기
         * @exception NullReferenceException
         * @exception InvalidCastException
         */
        Task<long> GetCountsAsync();


        /**
         * DOM 특정 값 가져오기
         * @param attributeName
         * @exception NullReferenceException
         */
        Task<String> GetAttributeAsync(String attributeName);


        /**
         * DOM 특정 값 주입
         * @param attributeName
         * @param attributeValue
         */
        Task SetAttributeAsync(String attributeName, String attributeValue);


        /**
         * 파싱용 웹 브라우저 주입
         * @param  webBrowser
         * @return Selector
         */
        Selector SetWebBrowser(ChromiumWebBrowser webBrowser);
    }
}
