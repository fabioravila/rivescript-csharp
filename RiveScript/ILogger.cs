using System;

namespace RiveScript
{
    public interface ILogger
    {
        void Debug(string text);
        void Warn(string text);
        void Warn(string text, string filenasme, int lineno);

        void Error(string text);
        void Trace(string text);

        void Error(Exception exception);

        bool IsDebugEnable { get; }
        bool IsWarnEnable { get; }
        bool IsTraceEnable { get; }
        bool IsErrorEnable { get; }
    }
}
