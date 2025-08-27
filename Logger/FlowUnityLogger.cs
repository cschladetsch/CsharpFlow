// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

namespace Flow.Impl
{
    public class FlowUnityLogger : FlowLoggerBase
    {
        public FlowUnityLogger() : base()
        {
        }

        public FlowUnityLogger(string prefix) : base(prefix)
        {
        }

        protected override ILogOutput CreateOutput()
        {
            return new UnityLogOutput();
        }

        protected override void Log(ELogLevel level, string message)
        {
#if UNITY
            // Unity-specific logging behavior
            var unityLevel = level switch
            {
                ELogLevel.Error => UnityEngine.LogType.Error,
                ELogLevel.Warn => UnityEngine.LogType.Warning,
                ELogLevel.Info => UnityEngine.LogType.Log,
                ELogLevel.Verbose => UnityEngine.LogType.Log,
                _ => UnityEngine.LogType.Log
            };

            var formattedMessage = Formatter.Format(level, message, Context);
            UnityEngine.Debug.unityLogger.Log(unityLevel, formattedMessage);
#else
            base.Log(level, message);
#endif
        }
    }
}