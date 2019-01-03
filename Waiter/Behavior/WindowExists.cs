using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.WinForms;

namespace CEFParser.Waiter.Behavior
{
    public class WindowExists : Behavior
    {
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32")]
        private static extern IntPtr FindWindowEx(IntPtr hWnd, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(HandleRef hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(HandleRef hWnd, StringBuilder lpString, int nMaxCount);

        private Form mainForm;

        private String className;

        private String caption;


        public WindowExists(Form form, String className, String caption)
        {
            this.mainForm = form;
            this.className = className;
            this.caption = caption;
        }


        /**
         * DOM 객체가 존재하는지 테스트
         */
        public async Task<bool> CheckAsync()
        {
            try
            {
                if (CheckWindowExists() != IntPtr.Zero)
                    return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private IntPtr CheckWindowExists()
        {
            IntPtr next = FindWindow(this.className, null);

            int capacity;
            StringBuilder stringBuilder;

            while (next != IntPtr.Zero)
            {
                capacity = GetWindowTextLength(new HandleRef(mainForm, next)) * 2;
                stringBuilder = new StringBuilder(capacity);
                GetWindowText(new HandleRef(mainForm, next), stringBuilder, stringBuilder.Capacity);

                if (stringBuilder.ToString().IndexOf(this.caption) > -1) return next;
                next = FindWindowEx(IntPtr.Zero, next, this.className, null);
            }

            return IntPtr.Zero;
        }

        public void SetWebBrowser(ChromiumWebBrowser webBrowser)
        {
            //
        }
    }
}
