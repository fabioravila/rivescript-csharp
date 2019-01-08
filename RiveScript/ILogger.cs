using System;

namespace RiveScript
{
    public interface ILogger
    {
        void debug(string text);
        void warn(string text);
        void warn(string text, string filenasme, int lineno);

        void error(string text);
        void trace(string text);

        void error(Exception exception);

        bool isDebugEnable { get; }
        bool isWarnEnable { get; }
        bool isTraceEnable { get; }
        bool isErrorEnable { get; }
    }
}
