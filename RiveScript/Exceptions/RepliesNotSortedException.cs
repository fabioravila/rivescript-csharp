namespace RiveScript.Exceptions
{
    /// <summary>
    /// Thrown to indicate the replies are not sorted.
    /// </summary>
    public class RepliesNotSortedException : RiveScriptException
    {

        public RepliesNotSortedException() : base() { }
        public RepliesNotSortedException(string message) : base(message) { }
    }
}
