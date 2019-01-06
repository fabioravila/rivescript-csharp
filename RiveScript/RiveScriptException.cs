using System;

namespace RiveScript
{
    public class RiveScriptException : Exception
    {
        public RiveScriptException() : base() { }

        public RiveScriptException(string message) : base(message) { }

        public RiveScriptException(string message, Exception innerException) :
            base(message, innerException)
        { }

    }
}
