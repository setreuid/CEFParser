using CEFParser.Waiter.Selector;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEFParser.Waiter.Behavior
{
    public class ElementNotEqualsInnerHTML : Behavior
    {
        private Selector.Selector selector;

        private String htmlString;


        public ElementNotEqualsInnerHTML(Selector.Selector targetSelector, String htmlString)
        {
            this.selector = targetSelector;
            this.htmlString = htmlString;
        }


        public async Task<bool> CheckAsync()
        {
            try
            {
                if (await ((CSS)selector).GetAttributeAsync("innerHTML") != htmlString)
                    return true;
                return false;
            }
            catch (Exception e1)
            {
                Trace.WriteLine(e1.Message);
                return false;
            }
        }


        public void SetWebBrowser(ChromiumWebBrowser webBrowser)
        {
            this.selector.SetWebBrowser(webBrowser);
        }
    }
}
