using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace CodeTest
{
    /// <summary>
    /// File operation handler
    /// </summary>
    /// <remarks>Comments are on interface</remarks>
    internal class FileManager : IFileManager
    {
        private readonly object _syncObject;

        public string BaseFolder { get; private set; }
        internal string TraceFile => System.IO.Path.Combine(BaseFolder, "trace.log");

        public FileManager(string baseFolder)
        {
            if (string.IsNullOrEmpty(baseFolder))
                throw new ArgumentNullException(nameof(baseFolder));

            // let it fail if it has IO exception due to incorrect path etc
            if ( System.IO.Directory.Exists(baseFolder))
            {
                System.IO.Directory.Delete(baseFolder, true); // we shall create it again later
            }
            System.IO.Directory.CreateDirectory(baseFolder);

            BaseFolder = baseFolder.Trim('\\') + "\\"; // always has trailing slash at end
            _syncObject = new object();
        }

        #region Members
        public void Save(string fileLocation, byte[] data)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));
                if (string.IsNullOrEmpty(fileLocation))
                    throw new ArgumentNullException(nameof(fileLocation));

                File.WriteAllBytes(fileLocation, data);
            }
            catch (System.IO.IOException)
            {
                // we swallow because some files have special names that are not stored without special modification. I skip those files
                Trace("Special Case: file {0} will not be saved because it might contain invalid characters", fileLocation);
            }
        }

        public void Trace(string format, params object[] args)
        {
            lock (_syncObject)
            {
                File.AppendAllText(TraceFile,
                    string.Format("[T#{0}] ", Thread.CurrentThread.ManagedThreadId) +
                    string.Format(format + "\r\n", args));
            }
        }

        public void TraceError(string function, Exception ex)
        {
            Trace("Error: {0}\r\nStackTrace: {1}", ex.Message, ex.StackTrace);
        }

        public string GetWhereToSaveFile(string fileToDownload)
        {
            if (string.IsNullOrEmpty(fileToDownload))
                throw new ArgumentNullException(nameof(fileToDownload));

            lock (_syncObject)
            {
                // regex, I assume the file to download should always be a http://something.com like link so I try to get the something.com as file name
                // it can be also http://something.com/images/exit.png which should be fine and should get something.com/images/exit.png
                // NOTE: I ignore valid file path check here, it is too much of work to understand what is accepted or not
                MatchCollection listingMatches = Regex.Matches(fileToDownload, "(?<=\\/\\/)[^\\?]*", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                string value = fileToDownload;
                if (listingMatches.Count > 0)
                {
                    value = listingMatches[0].Value;
                }

                value = value.Replace('/', '\\');
                value = BaseFolder + value.TrimStart('\\'); // this is file path, trim the path if it has \ already to avoid double slash

                // special case, link is to a folder and we shall try to create a file with a / at end. instead lets make it index.html under that folder
                if (value.EndsWith("\\"))
                {
                    value += "index.html";
                }

                string dirPath = System.IO.Path.GetDirectoryName(value);

                // special case, I did not do a special index page creation thingy so SITE/meet and SITE/meet/someone are valid files for me but of course not for windows. 
                // hack: rename file to XXX.html to open space for folder
                if (System.IO.File.Exists(dirPath))
                {
                    System.IO.File.Move(dirPath, dirPath + ".html");
                }

                // we also create the folder for the file to store, like recursively
                System.IO.Directory.CreateDirectory(dirPath);

                return value;
            }
        }

        public string ReadAllText(string filePath)
        {
            lock( _syncObject)
            {
                return System.IO.File.ReadAllText(filePath);
            }
        }

        #endregion
    }
}
