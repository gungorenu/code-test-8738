using System;
using System.IO;

namespace CodeTest
{
    internal class FileManager : IFileManager
    {
        public string BaseFolder => Environment.CurrentDirectory;
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

    }
}
