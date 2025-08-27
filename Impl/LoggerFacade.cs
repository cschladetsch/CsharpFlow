// (C) 2012 christian.schladetsch@gmail.com See https://github.com/cschladetsch/Flow.

namespace Flow {
    /// <summary>
    ///     DOC
    /// </summary>
    /// <typeparam name="TLogger"></typeparam>
    public class LoggerFacade<TLogger>
        : ILogger where TLogger
        : class
        , ILogger
        , new() {
        private readonly TLogger _log = new TLogger();

        public LoggerFacade(string pre) {
            _log.LogPrefix = pre;
            _log.ShowStack = false;
            _log.ShowSource = true;
        }

        public string LogPrefix {
            get => _log.LogPrefix;
            set => _log.LogPrefix = value;
        }

        public object LogSubject {
            get => _log.LogSubject;
            set => _log.LogSubject = value;
        }

        public bool ShowSource {
            get => _log.ShowSource;
            set => _log.ShowSource = value;
        }

        public bool ShowStack {
            get => _log.ShowStack;
            set => _log.ShowStack = value;
        }

        public int Verbosity {
            get => _log.Verbosity;
            set => _log.Verbosity = value;
        }

        public void Info(string fmt, params object[] args) {
            _log.Info(fmt, args);
        }

        public void Warn(string fmt, params object[] args) {
            _log.Warn(fmt, args);
        }

        public void Error(string fmt, params object[] args) {
            _log.Error(fmt, args);
        }

        public void Verbose(int level, string fmt, params object[] args) {
            _log.Verbose(level, fmt, args);
        }
    }
}