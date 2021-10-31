using System;
using System.Collections.Generic;
using System.Text;

namespace CodeTest
{
    internal interface IFileManager
    {
        string BaseFolder { get; }

        void Save(string fileLocation, byte[] data); // data type?

        void Trace(string format, params object[] args);

        void TraceError(string function, Exception ex);
    }
}
