using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CEFParser.Waiter;
using CefSharp.WinForms;

namespace CEFParser.Watcher
{
    /**
     * HTML DOM 객체를 확인하는 Waiter 객체의
     * 주기적 수행을 위한 클래스
     */
    public class Watcher
    {
        private ChromiumWebBrowser webBrowser;

        // 파싱시도 전에 Element 체크를 위한 주기 타이머
        private System.Timers.Timer mainTimer;

        // 타이머가 Element 검색을 수행할 주기
        private static double timerInterval = 1000; // 1 Sec

        private List<Waiter.Waiter> checkList;

        private Mutex taskMutex;

        private long now;

        
        public Watcher(ChromiumWebBrowser targetBrowser, Mutex mutex)
        {
            this.webBrowser = targetBrowser;
            this.mainTimer = new System.Timers.Timer();
            this.checkList = new List<Waiter.Waiter>();
            this.taskMutex = mutex;
            
            // Watcher timer
            this.mainTimer.Interval = timerInterval;
            this.mainTimer.Enabled = true;
            this.mainTimer.Elapsed += this.DoCheck;
        }


        ~Watcher()
        {
            this.taskMutex.Close();
            this.taskMutex.Dispose();
            this.taskMutex = null;

            this.mainTimer.Enabled = false;
            this.mainTimer.Dispose();
            this.mainTimer = null;

            this.checkList.Clear();
            this.checkList = null;
        }


        /**
         * 주기적 체크 수행
         * @param sender
         * @param e
         */
        private void DoCheck(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                this.webBrowser.Invoke(new Action(async () =>
                {

                    this.taskMutex.WaitOne();
                    this.now = DateTime.Now.Ticks;

                    Trace.WriteLine(this.now);

                    // 일시적으로 타이머 멈춤
                    this.mainTimer.Enabled = false;
                    
                    for (int i=0; i< this.checkList.Count; i++)
                    {
                        Waiter.Waiter waiter = this.checkList.FirstOrDefault();

                        if (await waiter.CheckAsync())
                        {
                            // 체크 성공시
                            this.checkList.Remove(waiter);
                            waiter.Resolve();
                        }
                        else if (waiter.IsTimeout(now))
                        {
                            this.checkList.Remove(waiter);
                            waiter.Reject(Waiter.RejectReason.TIMEOUT);
                        }
                    }

                    this.taskMutex.ReleaseMutex();

                    this.mainTimer.Enabled = true;
                }));
            }
            catch (Exception) { }
        }


        /**
         * 작업 추가
         * @param waiter
         */
        public void Add(Waiter.Waiter waiter)
        {
            this.taskMutex.WaitOne();
            this.checkList.Add(waiter);
            this.taskMutex.ReleaseMutex();
        }
    }
}
