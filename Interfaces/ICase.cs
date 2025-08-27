// (C) 2012 christian.schladetsch@gmail.com. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow {
    public interface ICase<in T>
        where T
        : IComparable<T> {
        IGenerator Body { get; }
        bool Matches(T val);
    }
}