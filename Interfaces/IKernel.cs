// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow {
    /// <inheritdoc />
    /// <summary>
    ///     When an IKernel is Stepped, it updates its Time property,
    ///     and steps its Root node.
    /// </summary>
    public interface IKernel
        : IGenerator {
        EDebugLevel DebugLevel { get; set; }
        ILogger Log { get; set; }

        /// <summary>
        ///     If true, the Kernel will stop mid-senten
        /// </summary>
        bool Break { get; }

        /// <summary>
        ///     The root of the process tree.
        /// </summary>
        INode Root { get; set; }

        IFactory Factory { get; }
        ITimeFrame Time { get; }

        void Update(float deltaSeconds);
        void Wait(TimeSpan duration);
        void BreakFlow();
    }
}