// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow {
    /// <summary>
    ///     Stores information about a time step.
    /// </summary>
    public interface ITimeFrame {
        DateTime Last { get; }
        DateTime Now { get; }
        TimeSpan Delta { get; }
    }
}