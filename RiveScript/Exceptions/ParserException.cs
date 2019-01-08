using System;

namespace RiveScript.Exceptions
{
    /// <summary>
    /// Thrown to indicate an error during parsing.
    /// </summary>
    public class ParserException : RiveScriptException
    {

        public ParserException() : base() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception innerException) : base(message, innerException) { }
    }
}
