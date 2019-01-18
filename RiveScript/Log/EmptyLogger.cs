using System;

namespace RiveScript.Log
{
    public class EmptyLogger : ILogger
    {
        public bool IsTraceEnable => true;
        public bool IsDebugEnable => true;
        public bool IsWarnEnable => true;
        public bool IsErrorEnable => true;

        public void Debug(string text)
        {
            //NOTHING
        }

        public void Error(string text)
        {
            //NOTHING
        }

        public void Error(Exception exception)
        {
            //NOTHING
        }

        public void Trace(string text)
        {
            //NOTHING
        }

        public void trace(Exception exception)
        {
            //NOTHING
        }

        public void Warn(string text)
        {
            //NOTHING
        }

        public void Warn(string text, string filenasme, int lineno)
        {
            //NOTHING
        }
    }
}