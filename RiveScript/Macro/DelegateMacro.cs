using System;

namespace RiveScript.Macro
{
    /// <summary>
    /// A class to fast create a native Subroutine macro
    /// </summary>
    public class DelegateMacro : ISubroutine
    {
        Func<RiveScriptEngine, string[], string> action;

        public DelegateMacro(Func<RiveScriptEngine, string[], string> action)
        {
            this.action = action;
        }

        public string Call(RiveScriptEngine rs, string[] args)
        {
            return action.Invoke(rs, args);
        }
    }
}
