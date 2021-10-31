using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CodeTest
{
    /// <summary>
    /// Console handler
    /// </summary>
    /// <remarks>Comments are on interface</remarks>
    internal class DownloadManager : IDownloadManager, IDownloadManagerTesting
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
            _webClient = webClient;

            _source = new CancellationTokenSource();
            _syncObject = new object();
            _toBeDownloadedFiles = new Queue<string>();
            _tasks = new List<Task>();
            _downloadedFiles = new HashSet<string>();
        }

        public void DownloadSite(string site, int threadCount, string folder)
        {
            if (string.IsNullOrEmpty(site))
                throw new ArgumentNullException(nameof(site));
            if (threadCount < 1 || threadCount > 100)
                throw new ArgumentException("Thread count is invalid, enter a number between 1 and 100");

            Reset();
            Initialize(site, threadCount);

            try
            {
                _tasks.ForEach((t) => t.Start());

                Task.WaitAll(_tasks.ToArray(), Token);
                _source.Cancel();
                _console.Log(ConsoleColor.Green, "\r\nAll done! Terminating...\n");
            }
            catch (OperationCanceledException)
            { }
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

        internal void Initialize(string site, int threadCount)
        {
            _siteToDownload = NormalizeWebSite(site);

            try
            {
                // first file is not async call
                byte[] indexData = _webClient.DownloadFile(_siteToDownload, Token);
                string indexFile = _fileMgr.GetWhereToSaveFile(_siteToDownload + "index.html");
                _fileMgr.Save(indexFile, indexData);

                InspectSite(indexFile, _siteToDownload);
                _downloadedFiles.Add(_siteToDownload);
            }
            catch (Exception ex)
            {
                _fileMgr.TraceError("DownloadSite", ex);
            }

            //for (int i = 0; i < threadCount; i++)
            //    _tasks.Add(new Task(DownloadFileThread, _source.Token));
        }

        /// <summary>
        /// Worker thread
        /// </summary>
        internal void DownloadFileThread()
        {
            // TODO
        }

        /// <summary>
        /// Inspects given file and adds more files to download if necessary
        /// </summary>
        /// <param name="fileToCheck">Full filepath</param>
        /// <param name="base">Base folder, needed for recursiveness</param>
        internal void InspectSite(string fileToCheck, string @base)
        {
            try
            {
                string content =  _fileMgr.ReadAllText(fileToCheck);
                string baseLink = @base;
                if (!baseLink.EndsWith("/"))
                {
                    baseLink = baseLink.Substring(0, baseLink.IndexOf("/"));
                }

                // simple regex to find all "href" links
                MatchCollection listingMatches = Regex.Matches(content, "(?<=href=(\"|'))(.+?)(?=(\"|'))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                foreach (Match m in listingMatches)
                {
                    string fileToDownload = "";

                    if (m.Value.StartsWith("/"))
                    {
                        fileToDownload = _siteToDownload + m.Value.Substring(1);
                    }
                    else if (m.Value.StartsWith(_siteToDownload))
                    {
                        fileToDownload = m.Value;
                    }
                    else
                    {
                        continue; // special links like google, facebook etc
                    }

                    if (ShouldDownloadFile(fileToDownload))
                        AddNewFileToQueue(fileToDownload);
                }
            }
            catch (Exception ex)
            {
                _fileMgr.TraceError("InspectSite", ex);
            }
        }

        /// <summary>
        /// Gets new file to download, for worker threads
        /// </summary>
        /// <returns>New file to download (uri)</returns>
        internal string PopNewFileFromQueue()
        {
            lock (_syncObject)
            {
                if (_toBeDownloadedFiles.Count == 0)
                    return null;
                else
                    return _toBeDownloadedFiles.Dequeue();
            }
        }

        /// <summary>
        /// Adds new file to download list, assuming we did not download it before
        /// </summary>
        /// <param name="newFile">New file to download</param>
        internal void AddNewFileToQueue(string newFile)
        {
            string normalized = newFile.Trim().ToLower();
            lock (_syncObject)
            {
                if (_downloadedFiles.Contains(normalized))
                    return;
                if (!_toBeDownloadedFiles.Contains(normalized))
                    _toBeDownloadedFiles.Enqueue(normalized);
            }
        }

        /// <summary>
        /// Normalizes the given web site for the first inspection
        /// </summary>
        /// <param name="site">Site to download</param>
        /// <returns>Normalized website, minor corrections</returns>
        internal string NormalizeWebSite(string site)
        {
            string value = site.ToLower();
            if (!value.EndsWith("/"))
                value += "/";

            if (!value.StartsWith("http://") && !value.StartsWith("https://"))
                value = "http://" + value;

            return value;
        }

        /// <summary>
        /// Filters the link if it is supposed to be downloaded or skipped
        /// </summary>
        /// <param name="link">File link</param>
        /// <returns>True if should be downloaded</returns>
        /// SPECIAL NOTE: I skip ../ links. it requires additional logic to ensure a file is downloaded once only
        internal bool ShouldDownloadFile(string link)
        {
            if (!link.StartsWith("/") && !link.Contains(_siteToDownload))
                return false;

            if (link.StartsWith("../"))
                return false;

            if (link.ToLower().StartsWith("http") && !link.Contains(_siteToDownload))
                return false;

            return true;
        }

        #endregion

        #region IDownloadManagerTesting Members

        Queue<string> IDownloadManagerTesting.ToBeDownloadedFiles => _toBeDownloadedFiles;

        HashSet<string> IDownloadManagerTesting.DownloadedFiles => _downloadedFiles;

        string IDownloadManagerTesting.Site { get => _siteToDownload; set => _siteToDownload = value; }
        #endregion

    }
}
