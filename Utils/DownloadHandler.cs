using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEFParser.Utils
{
    public class DownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        private List<String> ignores;

        private List<String> agrees;

        private String downloadSrc = String.Empty;


        public DownloadHandler SetFileIgnores(List<String> ignores)
        {
            this.ignores = ignores;
            return this;
        }


        public DownloadHandler SetFileAgrees(List<String> agrees)
        {
            this.agrees = agrees;
            return this;
        }


        public DownloadHandler SetDownloadSrc(String src)
        {
            this.downloadSrc = src;
            return this;
        }
        

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    // 직접 다운받아야 하는 파일의 경우 (ex: ActiveX)
                    if (this.agrees != null
                        && this.agrees.IndexOf(downloadItem.SuggestedFileName) > -1)
                    {
                        callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                    }

                    // 자동으로 받아지는 경우
                    else if (this.ignores == null
                        || this.ignores.IndexOf(downloadItem.SuggestedFileName) == -1)
                    {
                        OnBeforeDownloadFired?.Invoke(this, downloadItem);

                        // 다운로드할 경로를 설정했다면 별도 창 띄우지 않고 진행
                        if (downloadSrc != String.Empty)
                            callback.Continue(this.downloadSrc, showDialog: false);
                        else
                            callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                    }
                }
            }
        }


        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            if (downloadItem.IsComplete || downloadItem.IsCancelled)
            {
                OnDownloadUpdatedFired?.Invoke(this, downloadItem);
            }
        }
    }
}
