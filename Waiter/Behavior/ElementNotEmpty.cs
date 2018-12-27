using CEFParser.Waiter.Selector;
using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEFParser.Waiter.Behavior
{
    public class ElementNotEmpty : Behavior
    {
        private Selector.Selector selector;


        public ElementNotEmpty(Selector.Selector targetSelector)
        {
            this.selector = targetSelector;
        }


        public async Task<bool> CheckAsync()
        {
            try
            {
                if (await ((CSS)selector).GetAttributeAsync("innerHTML") != String.Empty)
                    return true;
                return false;
            }
            catch (Exception e1)
            {
                Trace.WriteLine(e1);
                return false;
            }
        }


        public void SetWebBrowser(ChromiumWebBrowser webBrowser)
        {
            this.selector.SetWebBrowser(webBrowser);
        }
    }
}
