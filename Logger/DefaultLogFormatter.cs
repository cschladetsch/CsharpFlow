// (C) 2012 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;
using System.Diagnostics;

namespace Flow.Impl
{
    public class DefaultLogFormatter : ILogFormatter
    {
        private static readonly string[] LogNames = { "Info", "Warn", "Error", "Verbose" };

        public string Format(ELogLevel level, string message, ILogContext context)
        {
            var timestamp = GetTimestamp(context);
            var prefix = GetPrefix(context);
            var subject = GetSubject(context);
            var levelMarker = GetLevelMarker(level);
            
            return $"{level}: {prefix}{timestamp} {subject}\n\t{levelMarker}{message}`";
        }

        public string FormatWithStackTrace(ELogLevel level, string message, ILogContext context, bool showSource)
        {
            if (!showSource)
                return "";

            var stackTrace = new StackTrace(true);
            var frames = stackTrace.GetFrames();
            var result = "";
            var lead = "\t\t";
            bool foundTop = false;

            foreach (var frame in frames)
            {
                if (!foundTop)
                {
                    var methodName = frame.GetMethod().Name;
                    if (Array.IndexOf(LogNames, methodName) >= 0)
                    {
                        foundTop = true;
                        continue;
                    }
                }

                if (!foundTop)
                    continue;

                var fileName = frame.GetFileName();
                if (string.IsNullOrEmpty(fileName))
                    break;

                result += $"{lead}{fileName}({frame.GetFileLineNumber()},{frame.GetFileColumnNumber()}): from: {frame.GetMethod().Name}\n";

                if (!context.ShowStack)
                    break;

                lead += "\t";
            }

            return result.TrimEnd('\n');
        }

        private static string GetTimestamp(ILogContext context)
        {
            var elapsed = DateTime.Now - context.StartTime;
            var ms = elapsed.ToString(@"fff");
            return elapsed.ToString(@"mm\:ss\:") + ms;
        }

        private static string GetPrefix(ILogContext context)
        {
            return string.IsNullOrEmpty(context.Prefix) ? "" : $"{context.Prefix}: ";
        }

        private static string GetSubject(ILogContext context)
        {
            var named = context.Subject as INamed;
            var name = named?.Name ?? "";
            var from = string.IsNullOrEmpty(name) ? "" : $" {name}:";
            
            var gen = context.Subject as IGenerator;
            var step = gen == null ? "" : $"#{gen.StepNumber}/{gen.Kernel?.StepNumber}: ";
            
            return step + from;
        }

        private static string GetLevelMarker(ELogLevel level)
        {
            return level switch
            {
                ELogLevel.Info => "`",
                ELogLevel.Warn => "``",
                ELogLevel.Error => "```",
                ELogLevel.Verbose => "````",
                _ => "`"
            };
        }
    }
}