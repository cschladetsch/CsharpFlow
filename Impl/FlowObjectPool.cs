// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Flow.Impl
{
    public class FlowObjectPool<T> : IFlowObjectPool<T> where T : class
    {
        private readonly ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();
        private readonly Func<T> _factory;
        private readonly Action<T> _resetAction;
        private readonly int _maxCapacity;
        private int _totalRented = 0;
        private int _totalReturned = 0;
        private bool _disposed = false;

        public FlowObjectPool(Func<T> factory, int maxCapacity = 100, Action<T> resetAction = null)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _maxCapacity = Math.Max(1, maxCapacity);
            _resetAction = resetAction ?? DefaultReset;
        }

        public int Count => _pool.Count;
        public int MaxCapacity => _maxCapacity;

        public T Rent()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FlowObjectPool<T>));

            System.Threading.Interlocked.Increment(ref _totalRented);

            if (_pool.TryDequeue(out var item))
            {
                return item;
            }

            return _factory();
        }

        public void Return(T item)
        {
            if (_disposed || item == null)
                return;

            try
            {
                _resetAction(item);
                
                if (_pool.Count < _maxCapacity)
                {
                    _pool.Enqueue(item);
                    System.Threading.Interlocked.Increment(ref _totalReturned);
                }
            }
            catch (Exception)
            {
                // If reset fails, don't return to pool
            }
        }

        public void Clear()
        {
            while (_pool.TryDequeue(out var item))
            {
                if (item is IDisposable disposable)
                {
                    try { disposable.Dispose(); } catch { }
                }
            }
        }

        public PoolStatistics GetStatistics()
        {
            return new PoolStatistics
            {
                TotalRented = _totalRented,
                TotalReturned = _totalReturned,
                PooledCount = _pool.Count,
                MaxCapacity = _maxCapacity
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            Clear();
        }

        private static void DefaultReset(T item)
        {
            if (item is IPoolable poolable)
            {
                poolable.Reset();
            }
        }
    }

    public class FlowPoolManager : IFlowPoolManager
    {
        private readonly ConcurrentDictionary<Type, object> _pools = new ConcurrentDictionary<Type, object>();
        private readonly object _lock = new object();
        private bool _disposed = false;

        public IFlowObjectPool<T> GetPool<T>() where T : class, new()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FlowPoolManager));

            return (IFlowObjectPool<T>)_pools.GetOrAdd(typeof(T), _ => new FlowObjectPool<T>(() => new T()));
        }

        public IFlowObjectPool<T> CreatePool<T>(Func<T> factory, int maxCapacity = 100) where T : class
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FlowPoolManager));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            var pool = new FlowObjectPool<T>(factory, maxCapacity);
            RegisterPool<T>(pool);
            return pool;
        }

        public void RegisterPool<T>(IFlowObjectPool<T> pool) where T : class
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(FlowPoolManager));

            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            _pools.AddOrUpdate(typeof(T), pool, (key, existing) =>
            {
                // Dispose the existing pool if it's different
                if (existing is IDisposable existingDisposable && !ReferenceEquals(existing, pool))
                {
                    try { existingDisposable.Dispose(); } catch { }
                }
                return pool;
            });
        }

        public void ClearAll()
        {
            lock (_lock)
            {
                foreach (var pool in _pools.Values)
                {
                    if (pool is IDisposable disposable)
                    {
                        try { disposable.Dispose(); } catch { }
                    }
                }
                _pools.Clear();
            }
        }

        public PoolManagerStatistics GetStatistics()
        {
            var totalPools = _pools.Count;
            var totalPooled = 0;
            var totalRented = 0;
            var totalReturned = 0;

            foreach (var poolObj in _pools.Values)
            {
                if (poolObj.GetType().IsGenericType)
                {
                    var getStatsMethod = poolObj.GetType().GetMethod("GetStatistics");
                    if (getStatsMethod?.Invoke(poolObj, null) is PoolStatistics stats)
                    {
                        totalPooled += stats.PooledCount;
                        totalRented += stats.TotalRented;
                        totalReturned += stats.TotalReturned;
                    }
                }
            }

            return new PoolManagerStatistics
            {
                TotalPools = totalPools,
                TotalObjectsPooled = totalPooled,
                TotalObjectsRented = totalRented,
                OverallHitRate = totalRented > 0 ? (double)totalReturned / totalRented : 0
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            ClearAll();
        }
    }

    // Poolable implementations for common Flow objects
    public abstract class PoolableGenerator : Generator, IPoolable
    {
        public virtual void Reset()
        {
            // Reset generator state
            Running = false;
            StepNumber = 0;
            Value = null;
        }

        public virtual bool IsPoolable => !Active;
    }

    public abstract class PoolableTransient : Transient, IPoolable
    {
        public virtual void Reset()
        {
            // Reset transient state
            Active = true;
            Name = null;
        }

        public virtual bool IsPoolable => !Active;
    }
}