// (C) 2012 christian.schladetsch@gmail.com See https://github.com/cschladetsch/Flow.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Flow {
    /// <summary>
    ///     Creates Flow instances that reside within a Kernel.
    ///     Instances created by a Factory are not automatically added to any process.
    /// </summary>
    public interface IFactory {
        IKernel Kernel { get; set; }

        /// <summary>
        ///     Make a new Group containing the given set of <see cref="ITransient" />s.
        ///     When a group is Stepped, nothing happens. <see cref="IGroup" />
        /// </summary>
        IGroup Group(IEnumerable<ITransient> gens);

        IGroup Group(params ITransient[] gens);

        /// <summary>
        ///     Make a new Node containing the given <see cref="IGenerator" />s.
        ///     When a <see cref="INode" /> is Stepped, it steps all contained generators.
        /// </summary>
        /// <param name="gens">The generator to add to the new Node.</param>
        /// <returns>A new prepared Node.</returns>
        INode Node(IEnumerable<IGenerator> gens);

        INode Node(params IGenerator[] gens);

        /// <summary>
        ///     Make a new timer that fires once, then Completes.
        /// </summary>
        /// <param name="interval">
        ///     The shortest time-span before this timer will Complete.
        /// </param>
        ITimer OneShotTimer(TimeSpan interval);

        ITimer OneShotTimer(TimeSpan interval, Action<ITransient> onElapsed);

        /// <summary>
        ///     Make a new timer that fires an event at given intervals.
        /// </summary>
        /// <param name="interval">The shortest time between subsequent events.</param>
        IPeriodic PeriodicTimer(TimeSpan interval);

        /// <summary>
        ///     Make a new barrier with given args.
        /// </summary>
        IBarrier Barrier(params ITransient[] args);

        IBarrier Barrier(IEnumerable<ITransient> args);

        /// <summary>
        ///     Make a new TimedBarrier with given args.
        /// </summary>
        /// <param name="span">The shortest time span before the Barrier Completes.</param>
        ITimedBarrier TimedBarrier(TimeSpan span, params ITransient[] args);

        ITimedBarrier TimedBarrier(TimeSpan span, IEnumerable<ITransient> args);

        /// <summary>
        ///     Make a new trigger with given args.
        /// </summary>
        ITrigger Trigger(params ITransient[] args);

        /// <summary>
        ///     Make a new TimedTrigger with given args.
        /// </summary>
        /// <param name="span">The shortest time before the trigger Completes.</param>
        ITimedTrigger TimedTrigger(TimeSpan span, params ITransient[] args);

        /// <summary>
        ///     Make a new future value of type T.
        /// </summary>
        IFuture<T> Future<T>();

        IFuture<T> Future<T>(T val);

        /// <summary>
        ///     Make a new timed promised future of type T.
        /// </summary>
        /// <param name="timeOut">The shortest time-span before the future Completes.</param>
        ITimedFuture<T> TimedFuture<T>(TimeSpan timeOut);

        ITimedFuture<T> TimedFuture<T>(TimeSpan timeOut, T val);

        /// <summary>
        ///     Make a new <see cref="ICoroutine" />
        /// </summary>
        ICoroutine Coroutine(Func<IGenerator, IEnumerator> fun);

        ICoroutine Coroutine<T0>(Func<IGenerator, T0, IEnumerator> fun, T0 t0);
        ICoroutine Coroutine<T0, T1>(Func<IGenerator, T0, T1, IEnumerator> fun, T0 t0, T1 t1);
        ICoroutine Coroutine<T0, T1, T2>(Func<IGenerator, T0, T1, T2, IEnumerator> fun, T0 t0, T1 t1, T2 t2);
        ICoroutine<TR> Coroutine<TR>(Func<IGenerator, IEnumerator<TR>> fun);
        ICoroutine<TR> Coroutine<TR, T0>(Func<IGenerator, T0, IEnumerator<TR>> fun, T0 t0);

        /// <summary>
        ///     Make a new <see cref="ISubroutine" />
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="fun"></param>
        /// <returns></returns>
        ISubroutine<TR> Subroutine<TR>(Func<IGenerator, TR> fun);

        ISubroutine<TR> Subroutine<TR, T0>(Func<IGenerator, T0, TR> fun, T0 t0);
        ISubroutine<TR> Subroutine<TR, T0, T1>(Func<IGenerator, T0, T1, TR> fun, T0 t0, T1 t1);
        ISubroutine<TR> Subroutine<TR, T0, T1, T2>(Func<IGenerator, T0, T1, T2, TR> fun, T0 t0, T1 t1, T2 t2);

        /// <summary>
        ///     Make a new channel with value-type T.
        /// </summary>
        /// <typeparam name="TR">The type of things stored in the Channel.</typeparam>
        IChannel<TR> Channel<TR>();

        IChannel<TR> Channel<TR>(IGenerator<TR> gen);

        /// <summary>
        ///     Make a <see cref="IGenerator" /> that does nothing, then Completes.
        /// </summary>
        IGenerator Nop();

        /// <summary>
        ///     Do something.
        /// </summary>
        /// <param name="act">What to do.</param>
        IGenerator Do(Action act);

        /// <summary>
        ///     Make a Generator that always returns the same value.
        /// </summary>
        /// <param name="val">The value to always return.</param>
        IGenerator<T> Value<T>(T val);

        /// <summary>
        ///     DOC
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        IGenerator<T> Expression<T>(Func<T> action);

        /// <summary>
        ///     Make a generator that steps the given @if generator if the given
        ///     predicate returns true.
        /// </summary>
        /// <param name="pred">The predicate to test.</param>
        /// <param name="if">What to do if the predicate returns true.</param>
        IGenerator If(Func<bool> pred, IGenerator @if);

        /// <summary>
        ///     Make a Generator that does one of two things, depending on
        ///     result of given predicate.
        /// </summary>
        /// <param name="pred">The predicate to test.</param>
        /// <param name="if">What to generate if the predicate is true.</param>
        /// <param name="else">What to generate if the predicate is false.</param>
        IGenerator IfElse(Func<bool> pred, IGenerator @if, IGenerator @else);

        /// <summary>
        ///     Make a <see cref="IGenerator" /> that steps zero or
        ///     more other generators while the given predicate is true.
        /// </summary>
        IGenerator While(Func<bool> pred, params IGenerator[] body);

        /// <summary>
        ///     Perform one Generator after the previous Generator Completes.
        /// </summary>
        ISequence Sequence(params IGenerator[] transients);

        ISequence Sequence(IEnumerable<IGenerator> transients);

        /// <summary>
        ///     DOC
        /// </summary>
        IGenerator Break();

        /// <summary>
        ///     DOC
        /// </summary>
        /// <param name="duration"></param>
        ITransient Wait(TimeSpan duration);

        /// <summary>
        ///     DOC
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="timeOut"></param>
        ITransient WaitFor(ITransient trans, TimeSpan timeOut);

        /// <summary>
        ///     Do meta-things.
        /// </summary>
        IGenerator SetDebugLevel(EDebugLevel level);

        IGenerator Log(string fmt, params object[] args);
        IGenerator Warn(string fmt, params object[] args);
        IGenerator Error(string fmt, params object[] args);

        T Prepare<T>(T obj)
            where T : ITransient;
    }
}