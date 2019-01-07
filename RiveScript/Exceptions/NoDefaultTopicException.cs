namespace RiveScript.Exceptions
{
    public class NoDefaultTopicException : RiveScriptException
    {

        public NoDefaultTopicException() : base() { }

        public NoDefaultTopicException(string message) : base(message) { }
    }
}
