using System;
using System.Collections.Generic;
using System.Text;

namespace CodeTest
{
    /// <summary>
    /// Console handler, uses System.Console
    /// </summary>
    /// <remarks>Comments are on interface</remarks>
    internal class Console : IConsole
    {
        public bool IsVerbose { get; set; }

        public void Log(ConsoleColor color, string format, params object[] args)
        {
            System.Console.ForegroundColor = color;
            System.Console.Write(format, args);
        }
    }
}
