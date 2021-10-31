using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private int _operatingThreads = 0; // special flag, increment if a file is being downloaded (not processed yet), as long as it is >0 or _toBeDownloadedFiles.C>0 operation must continue
        private bool _inProgress = false;

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

        public void DownloadSite(string site, int threadCount)
        {
            if (string.IsNullOrEmpty(site))
                throw new ArgumentNullException(nameof(site));
            if (threadCount < 1 || threadCount > 100)
                throw new ArgumentException("Thread count is invalid, enter a number between 1 and 100");

            if (_inProgress)
                return;

            _inProgress = true;

            Task listener = null;
            try
            {
                Reset();
                Initialize(site, threadCount);

                // lets calculate how long it takes
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                listener = Task.Factory.StartNew(ListenCancellationThread);

                // set up the progress bar
                _console.Log(ConsoleColor.Magenta, "Press enter key to stop operation\n");
                ProgressUpdate();

                _tasks.ForEach((t) => t.Start());

                Task.WaitAll(_tasks.ToArray(), Token);
                _source.Cancel();
                Stop();
                _console.Log(ConsoleColor.Green, "\r\nAll done in {0} seconds! Terminating...\n", watch.ElapsedMilliseconds / 1000);
            }
            catch (OperationCanceledException)
            {
                // some tasks are still downloading and cancellation called but we have to wait
                do
                {
                    // I COULD NOT DO THIS CANCELLATION PART
                    // main issue is to wait for others to wait to finalize gracefully. I do not know a better way to wait for them to finalize themselves
                    // in fact all necessary cancellation signals have been sent so just waiting
                    Thread.Sleep(250);
                } while (_tasks.Any(t => !t.IsCanceled && !t.IsCompleted));
            }
            finally
            {
                _inProgress = false;

                // dirty trick, we need to dispose listener as well but should not wait for it because of Console.Readline behavior
                if (listener != null)
                {
                    _tasks.Add(listener);
                }
            }
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
            _operatingThreads = 0;
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

                for (int i = 0; i < threadCount; i++)
                    _tasks.Add(new Task(DownloadFileThread, _source.Token));

                _console.Log(ConsoleColor.Green, "Site inspection completed. Starting download process...");
            }
            catch (Exception ex)
            {
                _fileMgr.TraceError("DownloadSite", ex);
                throw new Exception("Downloading the index page failed. Operation cannot continue", ex);
            }
        }

        /// <summary>
        /// Worker thread
        /// </summary>
        internal void DownloadFileThread()
        {
            try
            {
                // our thread loop
                do
                {
                    // operation cancelled
                    if (_source.Token.IsCancellationRequested)
                        break;

                    // multiple things to check
                    string fileToDownload = PopNewFileFromQueue();
                    if (string.IsNullOrEmpty(fileToDownload))
                    {
                        // another file download is ongoing, lets wait for it instead of stopping
                        if (_operatingThreads > 0)
                        {
                            // Thread.Sleep is bad but for simplicity I avoid more complex structure to ping all threads if they are waiting already
                            // MRE can be used to ping threads but for this simple task not so necessary
                            Thread.Sleep(100);
                            continue;
                        }
                        // there is no file to download, everything seems downloaded, gracefully stop
                        else break;
                    }

                    // file already downloaded
                    if (_downloadedFiles.Contains(fileToDownload))
                        continue;

                    try
                    {
                        Interlocked.Increment(ref _operatingThreads);

                        // where shall we store the file, also sets up the folder structure
                        string whereToStore = _fileMgr.GetWhereToSaveFile(fileToDownload);
                        _fileMgr.Trace("Thread [{0}], File: {1}, To-Be-Saved-To: {2}, Downloading...", Thread.CurrentThread.ManagedThreadId, fileToDownload, whereToStore);
                        byte[] data = _webClient.DownloadFile(fileToDownload, _source.Token);
                        _fileMgr.Trace("Thread [{0}], File: {1}, {2} bytes downloaded, now saving and inspecting...", Thread.CurrentThread.ManagedThreadId, fileToDownload, data?.Length);

                        // regardless of success, we mark it as downloaded
                        MarkFileDownloaded(fileToDownload);

                        // we save the file, sometimes some links are queries and we cannot get a response, then data is null, we skip them
                        if (data != null)
                        {
                            _fileMgr.Save(whereToStore, data);

                            // inspect file to see if there are further files to download, it shall find base of the downloaded file to form the URI
                            InspectSite(whereToStore, fileToDownload);
                        }
                    }
                    finally
                    {
                        // thread moves to next task if there is any
                        Interlocked.Decrement(ref _operatingThreads);
                    }
                } while (true);
            }
            catch (OperationCanceledException)
            {
                // cancellation token ticked
            }
            catch (Exception ex)
            {
                _fileMgr.Trace("DownloadFileThread encountered an error which caused thread to stop. Witness me brother!");
                _fileMgr.TraceError("DownloadFileThread", ex);
            }
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
                string content = _fileMgr.ReadAllText(fileToCheck);
                string baseLink = @base;
                if (!baseLink.EndsWith("/"))
                {
                    baseLink = baseLink.Substring(0, baseLink.IndexOf("/"));
                }

                // simple regex to find all "href" links
                MatchCollection listingMatches = Regex.Matches(content, "(?<=href=(\\\"|'))(.+?)(?=(\\\"|'|\\?))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
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

                    string normalized = fileToDownload.Trim().ToLower();
                    if (ShouldDownloadFile(normalized))
                        AddNewFileToQueue(normalized);
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
            lock (_syncObject)
            {
                if (_downloadedFiles.Contains(newFile))
                    return;
                if (!_toBeDownloadedFiles.Contains(newFile))
                    _toBeDownloadedFiles.Enqueue(newFile);
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

            if (link.StartsWith("http") && !link.Contains(_siteToDownload))
                return false;

            return true;
        }

        /// <summary>
        /// Update progress on console
        /// </summary>
        /// <param name="message">Message, like file downloaded etc</param>
        internal void ProgressUpdate(string message = null)
        {
            int current = 0;
            int total = 0;
            lock (_syncObject)
            {
                current = _downloadedFiles.Count;
                total = _downloadedFiles.Count + _toBeDownloadedFiles.Count;
            }

            double progress = (20 * current) / total; // it is calculated, not incremented. we know how many files we downloaded and we shall but one problem is dynamic files
                                                      // for simplicity I do one x per %5
                                                      // at every file download we might add to our progress so the target file count can also increase
            int progressIndicator = Convert.ToInt32(Math.Floor(progress));
            _console.Progress(ConsoleColor.Cyan, "\r..: {0}/{1} :.. [{2}{3}] {4}",
                current,
                total,
                new string('x', progressIndicator),
                new string(' ', 20 - progressIndicator),
                message);
        }

        /// <summary>
        /// Mark a file downloaded, handle collections and update progress
        /// </summary>
        /// <param name="fileToDownload">File full URI</param>
        internal void MarkFileDownloaded(string fileToDownload)
        {
            lock (_syncObject)
            {
                _downloadedFiles.Add(fileToDownload);
            }
            ProgressUpdate(fileToDownload);
        }

        /// <summary>
        /// Console readline thread, to cancel entire operation
        /// </summary>
        internal void ListenCancellationThread()
        {
            try
            {
                var str = System.Console.ReadLine();
                if (!_source.IsCancellationRequested)
                {
                    _source.Cancel();
                    _console.Log(ConsoleColor.Yellow, "\r\nOperation cancelled by user! Terminating...\n");
                }
            }
            catch (OperationCanceledException)
            {
                // cancelled
            }
            catch (Exception ex)
            {
                _fileMgr.TraceError("ListenCancellationThread", ex);
            }
        }

        #endregion

        #region Concole.Readline() Cancellor
        // below is not my solution, tested briefly, may fail to do what is required

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        const int VK_RETURN = 0x0D;
        const int WM_KEYDOWN = 0x100;

        /// <summary>
        /// Stops download operations by triggering cancel thread which shall trigger cancellation token
        /// </summary>
        public void Stop()
        {
            var hWnd = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            PostMessage(hWnd, WM_KEYDOWN, VK_RETURN, 0);
        }
        #endregion

        #region IDownloadManagerTesting Members

        Queue<string> IDownloadManagerTesting.ToBeDownloadedFiles => _toBeDownloadedFiles;

        HashSet<string> IDownloadManagerTesting.DownloadedFiles => _downloadedFiles;

        string IDownloadManagerTesting.Site { get => _siteToDownload; set => _siteToDownload = value; }
        #endregion

    }
}
