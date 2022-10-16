// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

namespace Flow.Impl
{
    internal class Break
        : Generator
        , IBreak
    {
        public override void Step()
        {
            Kernel.BreakFlow();
        }
    }
}
