namespace RiveScript.Exceptions
{
    /// <summary>
    /// Thrown to indicate no reply matched.
    /// </summary>
    public class ReplyNotMatchedException : RiveScriptException
    {

        public ReplyNotMatchedException() : base() { }
        public ReplyNotMatchedException(string message) : base(message) { }
    }
}
