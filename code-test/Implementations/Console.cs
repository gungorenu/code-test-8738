using System;
using System.Collections.Generic;
using System.Text;

namespace CodeTest
{
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
