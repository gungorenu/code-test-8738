using System;
using System.Collections.Generic;
using System.Text;

namespace CodeTest
{
    /// <summary>
    /// Console handler
    /// </summary>
    internal interface IConsole
    {
        /// <summary>
        /// Is verbose open
        /// </summary>
        bool IsVerbose { get; set; }

        /// <summary>
        /// Logs into console
        /// </summary>
        /// <param name="color">Console text color</param>
        /// <param name="format">Console message</param>
        /// <param name="args">Console message arguments</param>
        void Log(ConsoleColor color, string format, params object[] args);
    }
}
