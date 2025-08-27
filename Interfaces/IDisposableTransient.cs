// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow
{
    public interface IDisposableTransient : ITransient, IDisposable
    {
        event EventHandler<DisposingEventArgs> Disposing;
        bool IsDisposed { get; }
        void DisposeAfter(ITransient other);
    }

    public class DisposingEventArgs : EventArgs
    {
        public DisposingEventArgs(IDisposableTransient transient, DisposalReason reason)
        {
            Transient = transient;
            Reason = reason;
        }

        public IDisposableTransient Transient { get; }
        public DisposalReason Reason { get; }
    }

    public enum DisposalReason
    {
        Completed,
        Explicit,
        ParentDisposed,
        Timeout,
        Error
    }
}