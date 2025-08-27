// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow
{
    public interface IFlowObjectPool<T> : IDisposable where T : class
    {
        T Rent();
        void Return(T item);
        void Clear();
        int Count { get; }
        int MaxCapacity { get; }
        PoolStatistics GetStatistics();
    }

    public interface IPoolable
    {
        void Reset();
        bool IsPoolable { get; }
    }

    public interface IFlowPoolManager
    {
        IFlowObjectPool<T> GetPool<T>() where T : class, new();
        IFlowObjectPool<T> CreatePool<T>(Func<T> factory, int maxCapacity = 100) where T : class;
        void RegisterPool<T>(IFlowObjectPool<T> pool) where T : class;
        void ClearAll();
        PoolManagerStatistics GetStatistics();
    }

    public struct PoolStatistics
    {
        public int TotalRented { get; set; }
        public int TotalReturned { get; set; }
        public int CurrentlyRented => TotalRented - TotalReturned;
        public int PooledCount { get; set; }
        public int MaxCapacity { get; set; }
        public double HitRate => TotalRented > 0 ? (double)TotalReturned / TotalRented : 0;
    }

    public struct PoolManagerStatistics
    {
        public int TotalPools { get; set; }
        public int TotalObjectsPooled { get; set; }
        public int TotalObjectsRented { get; set; }
        public double OverallHitRate { get; set; }
    }
}