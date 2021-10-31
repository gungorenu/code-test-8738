using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CodeTest
{
    internal interface IDownloadManager
    {
        CancellationToken Token { get; }

        void DownloadSite(string site, int threadCount, string folder);
    }
}
