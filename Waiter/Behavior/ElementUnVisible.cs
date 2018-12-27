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
    public class ElementUnVisible : Behavior
    {
        private Selector.Selector selector;


        public ElementUnVisible(Selector.Selector targetSelector)
        {
            this.selector = targetSelector;
        }


        public async Task<bool> CheckAsync()
        {
            try
            {
                if (await ((CSS) selector).IsHiddenAsync())
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
