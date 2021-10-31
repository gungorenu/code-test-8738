using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeTest
{
    /// <summary>
    /// Console handler
    /// </summary>
    /// <remarks>Comments are on interface</remarks>
    internal class DownloadManager : IDownloadManager
    {
        #region Fields
        // referenced helpers
        private readonly IConsole _console;
        private readonly IFileManager _fileMgr;
        private readonly IWebClient _webClient;

        // syncobjects
        private readonly object _syncObject;
        private CancellationTokenSource _source;
        private double _progress;

        // download operation storage
        private readonly Queue<string> _toBeDownloadedFiles;
        private readonly HashSet<string> _downloadedFiles; // to avoid double download, all files
        private readonly List<Task> _tasks;
        private string _siteToDownload;
        #endregion

        public CancellationToken Token => _source.Token;

        public DownloadManager(IConsole console, IFileManager fileMgr, IWebClient webClient)
        {
            if (console == null)
                throw new ArgumentNullException(nameof(console));
            if (fileMgr == null)
                throw new ArgumentNullException(nameof(fileMgr));
            if (webClient == null)
                throw new ArgumentNullException(nameof(webClient));

            _console = console;
            _fileMgr = fileMgr;

            _source = new CancellationTokenSource();
            _syncObject = new object();
            _toBeDownloadedFiles = new Queue<string>();
            _tasks = new List<Task>();
            _downloadedFiles = new HashSet<string>();
        }

        public void DownloadSite(string site, int threadCount, string folder)
        {
            _console.Log(ConsoleColor.Gray, "Downloading site '{0}' with {1} threads", site, threadCount);
            _siteToDownload = site;
            // TODO
            //throw new NotImplementedException();
        }

        #region IDisposable
        public void Dispose()
        {
            if (_source != null)
            {
                _source.Dispose();
                _source = null;
            }

            _tasks.ForEach(t =>
            {
                try { t.Dispose(); } catch { }
            });
        }
        #endregion

        #region Internal Members
        internal void Reset()
        {
            _toBeDownloadedFiles.Clear();
            _tasks.Clear();
            _downloadedFiles.Clear();
            _progress = 0;
        }


        #endregion
    }
}
