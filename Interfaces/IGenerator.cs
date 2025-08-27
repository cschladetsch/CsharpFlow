// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow {
    public delegate void GeneratorHandler(IGenerator generator);

    /// <inheritdoc cref="ITransient" />
    /// <summary>
    ///     A Generator does some work every time its Step method is called, unless it is Suspended or Completed.
    ///     <para>All Generators are Resumed when they are first created by a Factory</para>
    /// </summary>
    public interface IGenerator
        : ITransient
            , ISteppable {
        bool Running { get; }
        int StepNumber { get; }
        object Value { get; }
        event GeneratorHandler Resumed;
        event GeneratorHandler Stepped;
        event GeneratorHandler Suspended;

        void Resume();
        void Pre();
        void Post();
        void Suspend();

        new IGenerator AddTo(IGroup group);
        new IGenerator Named(string name);

        IGenerator SuspendAfter(ITransient other);
        IGenerator SuspendAfter(TimeSpan span);

        IGenerator ResumeAfter(Func<bool> pred);
        IGenerator ResumeAfter(Func<bool> pred, string name);
        IGenerator ResumeAfter(ITransient other);
        IGenerator ResumeAfter(TimeSpan span);
    }

    public delegate void GeneratorHandler<in T>(IGenerator<T> generator);

    public interface IGenerator<out T>
        : IGenerator {
        new T Value { get; }
    }
}