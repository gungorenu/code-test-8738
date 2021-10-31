using System;

namespace CodeTest
{
    /// <summary>
    /// Console handler
    /// </summary>
    internal interface IConsole
    {
        /// <summary>
        /// Logs into console
        /// </summary>
        /// <param name="color">Console text color</param>
        /// <param name="format">Console message</param>
        /// <param name="args">Console message arguments</param>
        void Log(ConsoleColor color, string format, params object[] args);

        /// <summary>
        /// Logs progress data into console
        /// </summary>
        /// <param name="color">Console text color </param>
        /// <param name="format">Console message</param>
        /// <param name="args">Console message arguments</param>
        /// <remarks>Difference from Log is rewriting on line start</remarks>
        void Progress(ConsoleColor color, string format, params object[] args);
    }
}
