namespace RiveScript.Exceptions
{
    /// <summary>
    /// Thrown to indicate a deep recursion error
    /// </summary>
    public class DeepRecursonExcetion : RiveScriptException
    {
        public DeepRecursonExcetion() : base() { }

        public DeepRecursonExcetion(string message) : base(message) { }
    }
}
