using System;

namespace CodeTest
{
    /// <summary>
    /// Console handler, uses System.Console
    /// </summary>
    /// <remarks>Comments are on interface</remarks>
    internal class Console : IConsole
    {
        private object _syncObject;

        public Console()
        {
            _syncObject = new object();
        }

        public void Log(ConsoleColor color, string format, params object[] args)
        {
            lock (_syncObject)
            {
                var defaultColor = System.Console.ForegroundColor;
                try
                {
                    System.Console.ForegroundColor = color;
                    System.Console.Write(format, args);
                }
                finally
                {
                    System.Console.ForegroundColor = defaultColor;
                }
            }
        }

        public void Progress(ConsoleColor color, string format, params object[] args)
        {
            lock (_syncObject)
            {
                int currentLineCursor = System.Console.CursorTop;
                System.Console.SetCursorPosition(0, System.Console.CursorTop);
                System.Console.Write(new string(' ', System.Console.WindowWidth));
                System.Console.SetCursorPosition(0, currentLineCursor);

                var defaultColor = System.Console.ForegroundColor;
                try
                {
                    System.Console.ForegroundColor = color;
                    System.Console.Write(format, args);
                }
                finally
                {
                    System.Console.ForegroundColor = defaultColor;
                }
            }
        }
    }
}
