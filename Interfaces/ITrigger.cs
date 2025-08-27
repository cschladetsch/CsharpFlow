// (C) 2012 christian.schladetsch@gmail.com See https://github.com/cschladetsch/Flow.

using System;

namespace Flow {
    /// <inheritdoc />
    /// <summary>
    ///     A Trigger is a <see cref="T:Flow.IGroup" /> that completes whenever
    ///     a child completes.
    /// </summary>
    public interface ITrigger
        : IGroup {
        /// <summary>
        ///     What triggered this to Complete.
        /// </summary>
        ITransient Reason { get; }

        event Action<ITrigger, ITransient> OnTripped;
    }
}