using System;
using System.Collections.Generic;
using System.Text;

namespace CodeTest
{
    internal interface IConsole
    {
        bool IsVerbose { get; set; }

        void Log(ConsoleColor color, string format, params object[] args);
    }
}
