using System;
using System.Collections.Generic;
using System.Text;

namespace CodeTest
{
    internal class FileManager : IFileManager
    {
        public string BaseFolder => Environment.CurrentDirectory;

        public void Save(string fileLocation, byte[] data)
        {
            // TODO
            //throw new NotImplementedException();
        }

        public void Trace(string format, params object[] args)
        {
            // TODO
            //throw new NotImplementedException();
        }

        public void TraceError(string function, Exception ex)
        {
            Trace("Error: {0}\r\nStackTrace: {1}", ex.Message, ex.StackTrace);
        }
    }
}
