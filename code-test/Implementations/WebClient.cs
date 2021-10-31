using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace CodeTest
{
    /// <summary>
    /// Web client implementation, helper class, not thread safe
    /// </summary>
    /// <remarks>Comments are on interface</remarks>
    internal class WebClient : IWebClient
    {
        private readonly IFileManager _fileManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fileMgr">File manager instance</param>
        public WebClient(IFileManager fileMgr)
        {
            if (fileMgr == null)
                throw new ArgumentNullException(nameof(fileMgr));

            _fileManager = fileMgr;
        }

        public CancellationToken Token { get; set; }

        public byte[] DownloadFile(string fileToDownload)
        {
            byte[] data = null;
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    // token can be null, should just work fine
                    if (Token == null)
                    {
                        return client.DownloadData(new Uri(fileToDownload));
                    }
                    else
                    {
                        using (var reg = Token.Register(() => client.CancelAsync()))
                        {
                            data = client.DownloadData(new Uri(fileToDownload));
                            return data;
                        }
                    }
                }
            }
            catch (WebException wex) when (wex.Status == WebExceptionStatus.RequestCanceled)
            {
                // ignore this
            }
            catch (Exception ex)
            {
                _fileManager.TraceError("DownloadFile", ex);
            }

            return null;
        }
    }
}
