// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow
{
    public interface IFlowException
    {
        ITransient Context { get; }
        FlowErrorCode ErrorCode { get; }
        DateTime Timestamp { get; }
        string ComponentName { get; }
    }

    public interface IErrorRecovery
    {
        bool CanRecover(FlowException exception);
        ITransient CreateRecoveryAction(FlowException exception);
        void RecordRecoveryAttempt(FlowException exception, bool success);
    }

    public interface IFlowErrorHandler
    {
        bool HandleError(FlowException exception);
        void RegisterRecovery<T>(IErrorRecovery recovery) where T : FlowException;
        void UnregisterRecovery<T>() where T : FlowException;
    }

    public enum FlowErrorCode
    {
        Unknown = 0,
        
        // Kernel errors
        KernelNotInitialized = 1000,
        KernelAlreadyDisposed = 1001,
        KernelTimeoutExpired = 1002,
        
        // Generator errors
        GeneratorInvalidState = 2000,
        GeneratorAlreadyCompleted = 2001,
        GeneratorSuspendFailed = 2002,
        GeneratorResumeFailed = 2003,
        GeneratorStepFailed = 2004,
        
        // Factory errors
        FactoryCreationFailed = 3000,
        FactoryInvalidParameters = 3001,
        
        // Flow control errors
        BarrierTimeoutExpired = 4000,
        TriggerInvalidState = 4001,
        FutureTimeoutExpired = 4002,
        FutureValueNotSet = 4003,
        
        // Channel errors
        ChannelClosed = 5000,
        ChannelBufferFull = 5001,
        ChannelBufferEmpty = 5002,
        
        // Memory errors
        MemoryLeakDetected = 6000,
        DisposalFailed = 6001,
        ResourceExhausted = 6002,
        
        // Logger errors
        LoggerOutputFailed = 7000,
        LoggerFormattingFailed = 7001
    }
}