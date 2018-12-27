using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEFParser.Waiter
{
    public enum RejectReason
    {
        TIMEOUT
    }

    public delegate void TaskSuccess();

    public delegate void TaskReject(RejectReason reject);

    public class Waiter
    {
        private ChromiumWebBrowser webBrowser;

        private Behavior.Behavior behavior;
        
        private TaskSuccess taskSuccess;

        private TaskReject taskReject;

        private long inqueuedTime = 0;

        private long timeout = 0;


        public Waiter(Behavior.Behavior targetBehavior)
        {
            this.behavior = targetBehavior;
            this.inqueuedTime = DateTime.Now.Ticks;
        }

        public Waiter(Behavior.Behavior targetBehavior, TaskSuccess ts)
        {
            this.behavior = targetBehavior;
            this.inqueuedTime = DateTime.Now.Ticks;

            this.taskSuccess = ts;
        }

        public Waiter(Behavior.Behavior targetBehavior, TaskSuccess ts, TaskReject tr)
        {
            this.behavior = targetBehavior;
            this.inqueuedTime = DateTime.Now.Ticks;

            this.taskSuccess = ts;
            this.taskReject = tr;
        }

        public void SetWebBrowser(ChromiumWebBrowser webBrowser)
        {
            this.webBrowser = webBrowser;
            this.behavior.SetWebBrowser(webBrowser);
        }

        public async Task<bool> CheckAsync()
        {
            try
            {
                if (this.webBrowser == null)
                {
                    return false;
                }

                if (this.webBrowser.GetBrowser().GetFrameCount() < 1)
                {
                    return false;
                }

                return await this.behavior.CheckAsync();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Waiter Success(TaskSuccess success)
        {
            this.taskSuccess = success;
            return this;
        }

        public Waiter Catch(TaskReject reject)
        {
            this.taskReject = reject;
            return this;
        }

        public Waiter SetTimeout(long timeoutMilliSeconds)
        {
            this.timeout = timeoutMilliSeconds * 10000;
            return this;
        }
        
        public bool IsTimeout(long now)
        {
            if (timeout == 0) return false;
            return now - inqueuedTime > timeout;
        }

        public async void Resolve()
        {
            taskSuccess?.Invoke();
        }

        public async void Reject(RejectReason rejectReason)
        {
            taskReject?.Invoke(rejectReason);
        }
    }
}
