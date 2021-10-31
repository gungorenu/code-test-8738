using System;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("code-test-tests")]
namespace CodeTest
{
    internal static class Program
    {
        static void Main()
        {
            try
            {
                IConsole console = new Console();
                IFileManager fileMgr = new FileManager();
                IDownloadManager downloadMgr = new DownloadManager(console, fileMgr);

                System.Console.WriteLine("Enter a website to download: ");
                string site = System.Console.ReadLine();
                System.Console.WriteLine("Enter thread count (integer): ");
                string value = System.Console.ReadLine();
                int threadCount = 0;
                if(! int.TryParse(value, out threadCount))
                {
                    System.Console.WriteLine("Invalid integer type");
                    return;
                }

                downloadMgr.DownloadSite(site, threadCount, System.IO.Path.Combine( Environment.CurrentDirectory , "storage") );
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("High Level Error! If you are seeing this most probably something big messed up");
                System.Console.WriteLine("Error: {0}\r\nStacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
