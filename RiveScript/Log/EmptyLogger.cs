namespace RiveScript.Log
{
    public class EmptyLogger : ILogger
    {
        public bool isTraceEnable => true;
        public bool isDebugEnable => true;
        public bool isWarnEnable => true;
        public bool isErrorEnable => true;

        public void debug(string text)
        {
            //NOTHING
        }

        public void error(string text)
        {
            //NOTHING
        }

        public void trace(string text)
        {
            //NOTHING
        }

        public void warn(string text)
        {
            //NOTHING
        }
    }
}
