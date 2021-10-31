using System;
using System.Collections.Generic;
using System.Text;

namespace CodeTest
{
    /// <summary>
    /// File operation handler
    /// </summary>
    internal interface IFileManager
    {
        /// <summary>
        /// Base folder for storing files
        /// </summary>
        string BaseFolder { get; }

        /// <summary>
        /// Saves file data
        /// </summary>
        /// <param name="fileLocation">Location of file</param>
        /// <param name="data">Data to save</param>
        void Save(string fileLocation, byte[] data); // data type?

        /// <summary>
        /// Trace info, since console shall not be used for internal information
        /// </summary>
        /// <param name="format">Message format</param>
        /// <param name="args">Message arguments</param>
        void Trace(string format, params object[] args);

        /// <summary>
        /// Trace exception, since console shall not be used for internal information
        /// </summary>
        /// <param name="function">Function that failed</param>
        /// <param name="ex">Exception info</param>
        void TraceError(string function, Exception ex);
    }
}
