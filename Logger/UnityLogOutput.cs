// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

#if UNITY
using Debug = UnityEngine.Debug;
#else
using System;
#endif

namespace Flow.Impl
{
    public class UnityLogOutput : ILogOutput
    {
        public void Write(string message)
        {
#if UNITY
            Debug.Log(message);
#else
            Console.Write(message);
#endif
        }

        public void WriteLine(string message)
        {
#if UNITY
            Debug.Log(message);
#else
            Console.WriteLine(message);
#endif
        }

        public void Flush()
        {
            // Unity Debug doesn't require explicit flushing
#if !UNITY
            Console.Out.Flush();
#endif
        }
    }
}