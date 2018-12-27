using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEFParser.Waiter.Behavior
{
    public interface Behavior
    {
        void SetWebBrowser(ChromiumWebBrowser webBrowser);

        Task<bool> CheckAsync();
    }
}
