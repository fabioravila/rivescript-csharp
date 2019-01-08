namespace RiveScript.Exceptions
{
    /// <summary>
    /// Thrown to indicate no reply was found.
    /// </summary>
    public class ReplyNotFoundException : RiveScriptException
    {

        public ReplyNotFoundException() : base() { }
        public ReplyNotFoundException(string message) : base(message) { }
    }
}
