using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiveScript;
using RiveScript.lang;

namespace Playground
{
    public static class T
    {
        public static string M()
        {
            return "oi";
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            //It is an experience space for the library being developed

            var rs = new RiveScript.RiveScript();
            rs.setHandler("csharp", new CSharp());
            rs.stream(new[] { "",
                              "",
                              "",
                              "+ show context data",
                              "- data is: <call>context</call>",
                              "",
                              "> object context csharp",
                              "    using Playground;",
                              "    return T.M();",
                              "< object",
                              ""
            });


            rs.sortReplies();



            var result = rs.reply("default", "show context data");

        }
    }
}
