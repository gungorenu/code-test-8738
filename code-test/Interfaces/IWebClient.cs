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
        /// <returns>Data downloaded or null if error occurs</returns>
        byte[] DownloadFile(string fileToDownload);

        /// <summary>
        /// Cancellation token that shall be used
        /// </summary>
        /// <remarks>Can be null</remarks>
        CancellationToken Token { get; set; }
    }
}
