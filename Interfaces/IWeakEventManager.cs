// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow
{
    public interface IWeakEventManager
    {
        void Subscribe<T>(object target, string eventName, EventHandler<T> handler) where T : EventArgs;
        void Subscribe(object target, string eventName, EventHandler handler);
        void Unsubscribe(object target, string eventName, object handler);
        void UnsubscribeAll(object target);
        void Cleanup();
    }

    public interface IWeakEventHandler<T> where T : EventArgs
    {
        bool IsValid { get; }
        bool TryHandle(object sender, T args);
        void Invalidate();
    }
}