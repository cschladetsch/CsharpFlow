// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Flow.Impl
{
    public class WeakEventManager : IWeakEventManager
    {
        private readonly ConcurrentDictionary<WeakEventKey, List<IWeakEventHandler>> _subscriptions
            = new ConcurrentDictionary<WeakEventKey, List<IWeakEventHandler>>();

        public void Subscribe<T>(object target, string eventName, EventHandler<T> handler) where T : EventArgs
        {
            if (target == null || handler == null) return;

            var key = new WeakEventKey(target, eventName);
            var weakHandler = new WeakEventHandler<T>(handler.Target, handler.Method);
            
            _subscriptions.AddOrUpdate(key, 
                new List<IWeakEventHandler> { weakHandler },
                (k, list) => { list.Add(weakHandler); return list; });
        }

        public void Subscribe(object target, string eventName, EventHandler handler)
        {
            Subscribe<EventArgs>(target, eventName, (s, e) => handler?.Invoke(s, e));
        }

        public void Unsubscribe(object target, string eventName, object handler)
        {
            var key = new WeakEventKey(target, eventName);
            
            if (_subscriptions.TryGetValue(key, out var handlers))
            {
                handlers.RemoveAll(h => !h.IsValid || ReferenceEquals(h, handler));
                
                if (handlers.Count == 0)
                {
                    _subscriptions.TryRemove(key, out _);
                }
            }
        }

        public void UnsubscribeAll(object target)
        {
            var keysToRemove = new List<WeakEventKey>();
            
            foreach (var kvp in _subscriptions)
            {
                if (kvp.Key.TargetEquals(target))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _subscriptions.TryRemove(key, out _);
            }
        }

        public void Cleanup()
        {
            var keysToRemove = new List<WeakEventKey>();
            
            foreach (var kvp in _subscriptions)
            {
                kvp.Value.RemoveAll(h => !h.IsValid);
                
                if (kvp.Value.Count == 0 || !kvp.Key.IsTargetAlive())
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _subscriptions.TryRemove(key, out _);
            }
        }

        internal void RaiseEvent<T>(object sender, string eventName, T args) where T : EventArgs
        {
            var key = new WeakEventKey(sender, eventName);
            
            if (_subscriptions.TryGetValue(key, out var handlers))
            {
                var validHandlers = new List<IWeakEventHandler>();
                
                foreach (var handler in handlers)
                {
                    if (handler.IsValid)
                    {
                        handler.TryHandle(sender, args);
                        validHandlers.Add(handler);
                    }
                }
                
                if (validHandlers.Count != handlers.Count)
                {
                    _subscriptions.TryUpdate(key, validHandlers, handlers);
                }
            }
        }

        private struct WeakEventKey : IEquatable<WeakEventKey>
        {
            private readonly WeakReference _targetRef;
            private readonly string _eventName;
            private readonly int _hashCode;

            public WeakEventKey(object target, string eventName)
            {
                _targetRef = new WeakReference(target);
                _eventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
                _hashCode = RuntimeHelpers.GetHashCode(target) ^ eventName.GetHashCode();
            }

            public bool IsTargetAlive() => _targetRef.IsAlive;
            
            public bool TargetEquals(object target) => ReferenceEquals(_targetRef.Target, target);

            public bool Equals(WeakEventKey other)
            {
                return ReferenceEquals(_targetRef.Target, other._targetRef.Target) &&
                       _eventName == other._eventName;
            }

            public override bool Equals(object obj)
            {
                return obj is WeakEventKey other && Equals(other);
            }

            public override int GetHashCode() => _hashCode;
        }
    }

    public class WeakEventHandler<T> : IWeakEventHandler<T>, IWeakEventHandler where T : EventArgs
    {
        private readonly WeakReference _targetRef;
        private readonly MethodInfo _method;

        public WeakEventHandler(object target, MethodInfo method)
        {
            _targetRef = new WeakReference(target);
            _method = method ?? throw new ArgumentNullException(nameof(method));
        }

        public bool IsValid => _targetRef.IsAlive && _method != null;

        public bool TryHandle(object sender, T args)
        {
            var target = _targetRef.Target;
            if (target == null) return false;

            try
            {
                _method.Invoke(target, new object[] { sender, args });
                return true;
            }
            catch (Exception)
            {
                // Log error but don't throw
                return false;
            }
        }

        bool IWeakEventHandler.TryHandle(object sender, EventArgs args)
        {
            return args is T typedArgs && TryHandle(sender, typedArgs);
        }

        public void Invalidate()
        {
            _targetRef.Target = null;
        }
    }
}