namespace RiveScript.Exceptions
{
    /// <summary>
    /// Thrown to indicate no default topic exists.
    /// </summary>
    public class NoDefaultTopicException : RiveScriptException
    {

        public NoDefaultTopicException() : base() { }

        public NoDefaultTopicException(string message) : base(message) { }
    }
}
