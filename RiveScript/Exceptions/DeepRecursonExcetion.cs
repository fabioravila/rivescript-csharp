namespace RiveScript.Exceptions
{
    public class DeepRecursonExcetion : RiveScriptException
    {
        public DeepRecursonExcetion() : base() { }

        public DeepRecursonExcetion(string message) : base(message) { }
    }
}
