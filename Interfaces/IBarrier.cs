// (C) 2012 christian.schladetsch@gmail.com. See https://github.com/cschladetsch/Flow.

namespace Flow {
    /// <inheritdoc />
    /// <summary>
    ///     A Barrier is a <see cref="IGroup" /> that Completes itself when all added
    ///     <see cref="ITransient" />s have been removed from it.
    /// </summary>
    public interface IBarrier
        : IGroup {
        new IBarrier AddTo(IGroup group);
        new IBarrier Named(string name);
    }
}