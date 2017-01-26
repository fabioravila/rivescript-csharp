using RiveScript.lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiveScript
{
    public static class RiveScriptExtensions
    {
        public static void setCSharpHandler(this RiveScript rs)
        {
            rs.setHandler(Constants.CSharpHandlerName, new CSharp());
        }
    }
}
