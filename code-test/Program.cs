using System;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("code-test-tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")] // sometimes I hate this InternalsVisibleTo
namespace CodeTest
{
    internal static class Program
    {
        static void Main()
        {
            try
            {
                string storage = System.IO.Path.Combine(Environment.CurrentDirectory, "storage");

                IConsole console = new Console();
                IFileManager fileMgr = new FileManager(storage);
                IWebClient webClient = new WebClient(fileMgr);
                IDownloadManager downloadMgr = new DownloadManager(console, fileMgr, webClient);

                System.Console.WriteLine("Enter a website to download: ");
                string site = System.Console.ReadLine();
                System.Console.WriteLine("Enter thread count (integer): ");
                string value = System.Console.ReadLine();
                int threadCount = 0;
                if (!int.TryParse(value, out threadCount))
                {
                    System.Console.WriteLine("Invalid integer type");
                    return;
                }

                downloadMgr.DownloadSite(site, threadCount);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("High Level Error! If you are seeing this most probably something big messed up");
                System.Console.WriteLine("Error: {0}\r\nStacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
