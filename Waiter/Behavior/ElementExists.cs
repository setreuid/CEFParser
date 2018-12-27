using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEFParser.Waiter.Behavior
{
    public class ElementExists : Behavior
    {
        private Selector.Selector selector;


        public ElementExists(Selector.Selector targetSelector)
        {
            this.selector = targetSelector;
        }


        /**
         * DOM 객체가 존재하는지 테스트
         */
        public async Task<bool> CheckAsync()
        {
            try
            {
                if (await selector.GetCountsAsync() > 0)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetWebBrowser(ChromiumWebBrowser webBrowser)
        {
            this.selector.SetWebBrowser(webBrowser);
        }
    }
}
