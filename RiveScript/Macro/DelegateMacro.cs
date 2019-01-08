using System;

namespace RiveScript.Macro
{
    /// <summary>
    /// A class to fast create a native Subroutine macro
    /// </summary>
    public class DelegateMacro : ISubroutine
    {
        Func<RiveScript, string[], string> action;

        public DelegateMacro(Func<RiveScript, string[], string> action)
        {
            this.action = action;
        }

        public string Call(RiveScript rs, string[] args)
        {
            return action.Invoke(rs, args);
        }
    }
}
