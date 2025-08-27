// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;
using System.Runtime.Serialization;

namespace Flow.Impl
{
    [Serializable]
    public class FlowException : Exception, IFlowException
    {
        public FlowException(FlowErrorCode errorCode, ITransient context = null) 
            : base(GetDefaultMessage(errorCode))
        {
            ErrorCode = errorCode;
            Context = context;
            Timestamp = DateTime.UtcNow;
            ComponentName = context?.GetType().Name ?? "Unknown";
        }

        public FlowException(FlowErrorCode errorCode, string message, ITransient context = null) 
            : base(message)
        {
            ErrorCode = errorCode;
            Context = context;
            Timestamp = DateTime.UtcNow;
            ComponentName = context?.GetType().Name ?? "Unknown";
        }

        public FlowException(FlowErrorCode errorCode, string message, Exception innerException, ITransient context = null) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Context = context;
            Timestamp = DateTime.UtcNow;
            ComponentName = context?.GetType().Name ?? "Unknown";
        }

        protected FlowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ErrorCode = (FlowErrorCode)info.GetInt32(nameof(ErrorCode));
            Timestamp = info.GetDateTime(nameof(Timestamp));
            ComponentName = info.GetString(nameof(ComponentName));
        }

        public ITransient Context { get; }
        public FlowErrorCode ErrorCode { get; }
        public DateTime Timestamp { get; }
        public string ComponentName { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ErrorCode), (int)ErrorCode);
            info.AddValue(nameof(Timestamp), Timestamp);
            info.AddValue(nameof(ComponentName), ComponentName);
        }

        private static string GetDefaultMessage(FlowErrorCode errorCode)
        {
            return errorCode switch
            {
                FlowErrorCode.KernelNotInitialized => "Kernel has not been properly initialized",
                FlowErrorCode.KernelAlreadyDisposed => "Kernel has already been disposed",
                FlowErrorCode.GeneratorInvalidState => "Generator is in an invalid state for this operation",
                FlowErrorCode.GeneratorAlreadyCompleted => "Generator has already completed",
                FlowErrorCode.BarrierTimeoutExpired => "Barrier timeout has expired",
                FlowErrorCode.FutureTimeoutExpired => "Future timeout has expired",
                FlowErrorCode.FutureValueNotSet => "Future value has not been set",
                FlowErrorCode.ChannelClosed => "Channel has been closed",
                FlowErrorCode.MemoryLeakDetected => "Memory leak has been detected",
                _ => $"Flow error occurred: {errorCode}"
            };
        }
    }

    [Serializable]
    public class KernelException : FlowException
    {
        public KernelException(FlowErrorCode errorCode, IKernel kernel) 
            : base(errorCode, kernel as ITransient)
        {
            Kernel = kernel;
        }

        public KernelException(FlowErrorCode errorCode, string message, IKernel kernel) 
            : base(errorCode, message, kernel as ITransient)
        {
            Kernel = kernel;
        }

        public KernelException(FlowErrorCode errorCode, string message, Exception innerException, IKernel kernel) 
            : base(errorCode, message, innerException, kernel as ITransient)
        {
            Kernel = kernel;
        }

        protected KernelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IKernel Kernel { get; }
    }

    [Serializable]
    public class GeneratorException : FlowException
    {
        public GeneratorException(FlowErrorCode errorCode, IGenerator generator) 
            : base(errorCode, generator as ITransient)
        {
            Generator = generator;
        }

        public GeneratorException(FlowErrorCode errorCode, string message, IGenerator generator) 
            : base(errorCode, message, generator as ITransient)
        {
            Generator = generator;
        }

        public GeneratorException(FlowErrorCode errorCode, string message, Exception innerException, IGenerator generator) 
            : base(errorCode, message, innerException, generator as ITransient)
        {
            Generator = generator;
        }

        protected GeneratorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IGenerator Generator { get; }
    }

    [Serializable]
    public class ChannelException : FlowException
    {
        public ChannelException(FlowErrorCode errorCode, ITransient channel) 
            : base(errorCode, channel)
        {
            Channel = channel;
        }

        public ChannelException(FlowErrorCode errorCode, string message, ITransient channel) 
            : base(errorCode, message, channel)
        {
            Channel = channel;
        }

        public ChannelException(FlowErrorCode errorCode, string message, Exception innerException, ITransient channel) 
            : base(errorCode, message, innerException, channel)
        {
            Channel = channel;
        }

        protected ChannelException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ITransient Channel { get; }
    }

    [Serializable]
    public class FutureException : FlowException
    {
        public FutureException(FlowErrorCode errorCode, ITransient future) 
            : base(errorCode, future)
        {
            Future = future;
        }

        public FutureException(FlowErrorCode errorCode, string message, ITransient future) 
            : base(errorCode, message, future)
        {
            Future = future;
        }

        public FutureException(FlowErrorCode errorCode, string message, Exception innerException, ITransient future) 
            : base(errorCode, message, innerException, future)
        {
            Future = future;
        }

        protected FutureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ITransient Future { get; }
    }
}