// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;

namespace Flow.Impl
{
    public abstract class FlowLoggerBase : IFlowLogger
    {
        protected FlowLoggerBase()
        {
            Context = new LogContext();
            Formatter = new DefaultLogFormatter();
            Output = CreateOutput();
        }

        protected FlowLoggerBase(string prefix) : this()
        {
            Context.Prefix = prefix;
        }

        public ILogFormatter Formatter { get; set; }
        public ILogOutput Output { get; set; }
        public ILogContext Context { get; set; }

        public string LogPrefix
        {
            get => Context.Prefix;
            set => Context.Prefix = value;
        }

        public object LogSubject
        {
            get => Context.Subject;
            set => Context.Subject = value;
        }

        public int Verbosity { get; set; } = 5;

        public bool ShowSource
        {
            get => Context.ShowSource;
            set => Context.ShowSource = value;
        }

        public bool ShowStack
        {
            get => Context.ShowStack;
            set => Context.ShowStack = value;
        }

        public void Info(string fmt, params object[] args)
        {
            Log(ELogLevel.Info, SafeFormat(fmt, args));
        }

        public void Warn(string fmt, params object[] args)
        {
            Log(ELogLevel.Warn, SafeFormat(fmt, args));
        }

        public void Error(string fmt, params object[] args)
        {
            Log(ELogLevel.Error, SafeFormat(fmt, args));
        }

        public void Verbose(int level, string fmt, params object[] args)
        {
            if (level > Verbosity)
                return;

            Log(ELogLevel.Verbose, SafeFormat(fmt, args));
        }

        protected virtual void Log(ELogLevel level, string message)
        {
            try
            {
                var formattedMessage = Formatter.Format(level, message, Context);
                Output.WriteLine(formattedMessage);

                if (ShouldShowStackTrace(level))
                {
                    var stackTrace = Formatter.FormatWithStackTrace(level, message, Context, ShowSource);
                    Output.WriteLine(stackTrace);
                }

                Output.Flush();
            }
            catch (Exception ex)
            {
                // Fallback logging to prevent logger failures from breaking the application
                Console.WriteLine($"Logger Error: {ex.Message}");
                Console.WriteLine($"Original message: [{level}] {message}");
            }
        }

        protected virtual bool ShouldShowStackTrace(ELogLevel level)
        {
            return level == ELogLevel.Error || (ShowStack && ShowSource);
        }

        protected abstract ILogOutput CreateOutput();

        private static string SafeFormat(string fmt, object[] args)
        {
            if (fmt == null) return "Null";
            if (args == null || args.Length == 0) return fmt;

            try
            {
                return string.Format(fmt, args);
            }
            catch (FormatException)
            {
                return $"{fmt} [Format Error with {args.Length} args]";
            }
        }
    }

    public class LogContext : ILogContext
    {
        public LogContext()
        {
            StartTime = DateTime.Now;
        }

        public string Prefix { get; set; } = "";
        public object Subject { get; set; }
        public DateTime StartTime { get; }
        public IGenerator CurrentGenerator { get; set; }
        public bool ShowSource { get; set; } = true;
        public bool ShowStack { get; set; } = false;
    }
}