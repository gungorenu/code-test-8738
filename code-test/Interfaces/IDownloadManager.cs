using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CodeTest
{
    /// <summary>
    /// Main entry point for downloading a site
    /// </summary>
    internal interface IDownloadManager: IDisposable
    {
        /// <summary>
        /// Cancellation token
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// Downloads a site
        /// </summary>
        /// <param name="site">Site to download/traverse</param>
        /// <param name="threadCount">Thread count since it is parallel</param>
        void DownloadSite(string site, int threadCount);
    }

    /// <summary>
    /// Used only for testing
    /// </summary>
    internal interface IDownloadManagerTesting
    {
        Queue<string> ToBeDownloadedFiles { get; }
        HashSet<string> DownloadedFiles { get; }
        string Site { get; set; }
    }
}
