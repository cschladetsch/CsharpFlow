// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow {
    public delegate void FutureHandler<T>(IFuture<T> fut);

    /// <inheritdoc />
    /// <summary>
    ///     When its' Value property is first set, set Available property to true, and Complete.
    /// </summary>
    public interface IFuture<T>
        : ITransient {
        bool Available { get; }
        T Value { get; set; }
        event FutureHandler<T> Arrived;

        IFuture<T> Then(Action<IFuture<T>> action);
    }
}