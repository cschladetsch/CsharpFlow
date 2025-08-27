// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow
{
    public interface IFlowLogger : ILogger
    {
        ILogFormatter Formatter { get; set; }
        ILogOutput Output { get; set; }
        ILogContext Context { get; set; }
    }

    public interface ILogFormatter
    {
        string Format(ELogLevel level, string message, ILogContext context);
        string FormatWithStackTrace(ELogLevel level, string message, ILogContext context, bool showSource);
    }

    public interface ILogOutput
    {
        void Write(string message);
        void WriteLine(string message);
        void Flush();
    }

    public interface ILogContext
    {
        string Prefix { get; set; }
        object Subject { get; set; }
        DateTime StartTime { get; }
        IGenerator CurrentGenerator { get; set; }
        bool ShowSource { get; set; }
        bool ShowStack { get; set; }
    }

    public enum ELogOutputType
    {
        Console,
        Unity,
        File,
        Custom
    }
}