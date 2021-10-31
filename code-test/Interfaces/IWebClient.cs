using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CodeTest
{
    /// <summary>
    /// Handles Web operations
    /// </summary>
    public interface IWebClient
    {
        /// <summary>
        /// Downloads given file and returns data
        /// </summary>
        /// <param name="fileToDownload">File to download (web URI)</param>
        /// <param name="token">Token for cancellation</param>
        /// <returns>Data downloaded or null if error occurs</returns>
        byte[] DownloadFile(string fileToDownload, CancellationToken token = default(CancellationToken));
    }
}
