// (C) 2012 christian.schladetsch@gmail.com. See https://github.com/cschladetsch/Flow.

namespace Flow {
    /// <inheritdoc />
    /// <summary>
    ///     A buffered input/output stream.
    ///     <para>
    ///         If a Channel is created with a Generator, that is used as the
    ///         source for the channel.
    ///     </para>
    /// </summary>
    public interface IChannel<T>
        : ITransient {
        void Insert(T val);
        IFuture<T> Extract();

        new IChannel<T> AddTo(IGroup group);
    }
}