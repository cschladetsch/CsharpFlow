// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

namespace Flow.Impl
{
    public class FlowConsoleLogger : FlowLoggerBase
    {
        public FlowConsoleLogger() : base()
        {
        }

        public FlowConsoleLogger(string prefix) : base(prefix)
        {
        }

        protected override ILogOutput CreateOutput()
        {
            return new ConsoleLogOutput();
        }
    }
}