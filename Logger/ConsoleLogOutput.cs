// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;
using System.Diagnostics;

namespace Flow.Impl
{
    public class ConsoleLogOutput : ILogOutput
    {
        public void Write(string message)
        {
            Console.Write(message);
            Trace.Write(message);
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }

        public void Flush()
        {
            Console.Out.Flush();
            Trace.Flush();
        }
    }
}