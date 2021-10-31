using System;

namespace CodeTest
{
    internal static class Program
    {
        static void Main()
        {
            try
            {
                Console.WriteLine("Hello World!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("High Level Error! If you are seeing this most probably something big messed up");
                Console.WriteLine("Error: {0}\r\nStacktrace: {1}", ex.Message, ex.StackTrace);
            }
        }
    }
}
