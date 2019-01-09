using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.Log
{
    public class ConsoleLogger : ILogger
    {
        public enum LogLevel
        {
            Trace = 1,
            Debug = 2,
            Error = 3,
            Warn = 4
        }

        public bool Colored { get; set; } = true;
        public LogLevel Level { get; set; } = LogLevel.Warn;


        public bool IsTraceEnable => Level <= LogLevel.Trace;
        public bool IsDebugEnable => Level <= LogLevel.Debug;
        public bool IsErrorEnable => Level <= LogLevel.Error;
        public bool IsWarnEnable => Level <= LogLevel.Warn;

        public void Debug(string text)
        {
            if (!IsDebugEnable) return;

            Write(LogLevel.Debug, text);
        }

        public void Error(string text)
        {
            if (!IsErrorEnable) return;

            Write(LogLevel.Error, text);
        }

        public void Error(Exception exception)
        {
            if (!IsErrorEnable) return;

            Write(LogLevel.Error, exception.ToString());
        }

        public void Trace(string text)
        {
            if (!IsTraceEnable) return;

            Write(LogLevel.Trace, text);
        }

        public void Warn(string text)
        {
            if (!IsWarnEnable) return;

            Write(LogLevel.Warn, text);
        }

        public void Warn(string text, string filenasme, int lineno)
        {
            if (!IsWarnEnable) return;

            Write(LogLevel.Warn, text);
        }

        static void Write(LogLevel level, string message)
        {
            Console.ForegroundColor = GetColor(level);
            Console.WriteLine($"{GetSufix(level)} {message}");
            Console.ResetColor();
        }

        static string GetSufix(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace: return "[TRACE]";
                case LogLevel.Debug: return "[DEBUG]";
                case LogLevel.Error: return "[ERROR]";
                case LogLevel.Warn: return "[WARN ]";
                default: return "";
            }
        }

        static ConsoleColor GetColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return ConsoleColor.DarkBlue;
                case LogLevel.Error:
                    return ConsoleColor.DarkRed;
                case LogLevel.Warn:
                    return ConsoleColor.DarkYellow;
                case LogLevel.Debug:
                default:
                    return ConsoleColor.White;
            }
        }
    }
}
