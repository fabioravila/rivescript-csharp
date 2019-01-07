namespace RiveScript.Log
{
    public interface ILogger
    {
        void debug(string text);
        void trace(string text);
        void warn(string text);
        void warn(string text, string filenasme, int lineno);

        void error(string text);


        bool isTraceEnable { get; }
        bool isDebugEnable { get; }
        bool isWarnEnable { get; }
        bool isErrorEnable { get; }
    }
}
