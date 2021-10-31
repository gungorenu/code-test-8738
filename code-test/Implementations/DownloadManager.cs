using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace CodeTest
{
    internal class DownloadManager : IDownloadManager
    {
        private readonly IConsole _console;
        private readonly IFileManager _fileMgr;

        public CancellationToken Token => throw new NotImplementedException();

        public DownloadManager(IConsole console, IFileManager fileMgr)
        {
            _console = console;
            _fileMgr = fileMgr;
        }

        public void DownloadSite(string site, int threadCount, string folder)
        {
            _console.Log(ConsoleColor.Gray, "Downloading site '{0}' with {1} threads", site, threadCount);
            // TODO
            //throw new NotImplementedException();
        }

        internal byte[] DownloadFile(string fileToDownload, CancellationToken token)
        {
            byte[] data = null;
            try
            {
                using (var client = new WebClient())
                {
                    //using (var reg = token.Register(() => client.CancelAsync()))
                    {
                        data = client.DownloadData(new Uri(fileToDownload));
                        return data;
                    }
                }
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.RequestCanceled)
            {
                // ignore this
            }

            return null;
        }
    }
}
