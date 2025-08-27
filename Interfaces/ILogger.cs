// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

namespace Flow {
    /// <summary>
    ///     DOC
    /// </summary>
    public interface ILogger {
        string LogPrefix { get; set; }
        object LogSubject { get; set; }
        bool ShowSource { get; set; }
        bool ShowStack { get; set; }
        int Verbosity { get; set; }

        void Info(string fmt, params object[] args);
        void Warn(string fmt, params object[] args);
        void Error(string fmt, params object[] args);
        void Verbose(int level, string fmt, params object[] args);
    }
}