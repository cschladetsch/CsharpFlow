// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

namespace Flow {
    /// <inheritdoc />
    /// <summary>
    ///     An ISubroutine is-a IGenerator, implemented as a direct method call.
    /// </summary>
    public interface ISubroutine
        : IGenerator {
    }

    public interface ISubroutine<out T>
        : IGenerator<T> {
    }
}