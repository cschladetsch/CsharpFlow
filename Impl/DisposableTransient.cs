// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow.Impl
{
    public class DisposableTransient : Transient, IDisposableTransient
    {
        private bool _disposed = false;
        private readonly object _disposeLock = new object();

        public event EventHandler<DisposingEventArgs> Disposing;
        
        public bool IsDisposed 
        { 
            get 
            { 
                lock (_disposeLock) 
                { 
                    return _disposed; 
                } 
            } 
        }

        public void Dispose()
        {
            Dispose(DisposalReason.Explicit);
        }

        public void DisposeAfter(ITransient other)
        {
            if (other == null || !other.Active)
            {
                Dispose(DisposalReason.Completed);
                return;
            }

            void OnOtherCompleted(ITransient tr)
            {
                other.Completed -= OnOtherCompleted;
                Dispose(DisposalReason.Completed);
            }

            other.Completed += OnOtherCompleted;
        }

        protected virtual void Dispose(DisposalReason reason)
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            try
            {
                OnDisposing(reason);
                DisposeCore(reason);
            }
            catch (Exception ex)
            {
                // Log disposal errors but don't throw
                Kernel?.Log?.Error($"Error disposing {GetType().Name}: {ex.Message}");
            }
            finally
            {
                if (Active)
                {
                    Complete();
                }
            }
        }

        protected virtual void OnDisposing(DisposalReason reason)
        {
            Disposing?.Invoke(this, new DisposingEventArgs(this, reason));
        }

        protected virtual void DisposeCore(DisposalReason reason)
        {
            // Override in derived classes for specific disposal logic
        }

        public override void Complete()
        {
            if (!IsDisposed)
            {
                Dispose(DisposalReason.Completed);
            }
            else
            {
                base.Complete();
            }
        }

        ~DisposableTransient()
        {
            if (!_disposed)
            {
                Dispose(DisposalReason.Error);
            }
        }
    }
}