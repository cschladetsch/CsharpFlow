// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow
{
    /// <inheritdoc />
    /// <summary>
    /// A Trigger is a <see cref="T:Flow.IGroup" /> that completes whenever
    /// a child completes.
    /// </summary>
    public interface ITrigger
        : IGroup
    {
        event Action<ITrigger, ITransient> OnTripped;

        /// <summary>
        /// What triggered this to Complete.
        /// </summary>
        ITransient Reason { get; }
    }
}

