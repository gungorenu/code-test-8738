using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CodeTest
{
    /// <summary>
    /// File operation handler
    /// </summary>
    /// <remarks>Comments are on interface</remarks>
    internal class FileManager : IFileManager
    {
        public string BaseFolder => Environment.CurrentDirectory.Trim('\\') + "\\"; // always has trailing slash at end
        internal string TraceFile => System.IO.Path.Combine(BaseFolder, "trace.log");


        public void Save(string fileLocation, byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(fileLocation))
                throw new ArgumentNullException(nameof(fileLocation));

            File.WriteAllBytes(fileLocation, data);
        }

        public void Trace(string format, params object[] args)
        {
            File.AppendAllText(TraceFile,string.Format(format + "\r\n", args));
        }

        public void TraceError(string function, Exception ex)
        {
            Trace("Error: {0}\r\nStackTrace: {1}", ex.Message, ex.StackTrace);
        }

        public string GetWhereToSaveFile(string fileToDownload)
        {
            if (string.IsNullOrEmpty(fileToDownload))
                throw new ArgumentNullException(nameof(fileToDownload));

            // regex, I assume the file to download should always be a http://something.com like link so I try to get the something.com as file name
            // it can be also http://something.com/images/exit.png which should be fine and should get something.com/images/exit.png
            // NOTE: I ignore valid file path check here, it is too much of work to understand what is accepted or not
            MatchCollection listingMatches = Regex.Matches(fileToDownload, "(?<=\\/\\/).*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            string value = fileToDownload;
            if (listingMatches.Count > 0)
            {
                value = listingMatches[0].Value;
            }

            value = value.Replace('/', '\\');
            value = BaseFolder + value.TrimStart('\\'); // this is file path, trim the path if it has \ already to avoid double slash

            // we also create the folder for the file to store, like recursively
            string dirPath = System.IO.Path.GetDirectoryName(value);
            System.IO.Directory.CreateDirectory(dirPath);


            return value;
        }

    }
}
