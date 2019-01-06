using System;
using System.Threading.Tasks;
using System.Threading;

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

            //var rs = new RiveScript.RiveScript();
            //rs.setHandler("csharp", new CSharp());
            //rs.stream(new[] { "",
            //                  "",
            //                  "",
            //                  "+ show context data",
            //                  "- data is: <call>context</call>",
            //                  "",
            //                  "> object context csharp",
            //                  "    using Playground;",
            //                  "    return T.M();",
            //                  "< object",
            //                  ""
            //});


            //rs.stream(@"

            //             + turn (on|off) [the] light[s] [on|of|in] *
            //             - turning <star1> light on <star2>. {task:'turn on light <star2>'}
            //           ");

            //rs.sortReplies();



            //var result = rs.reply("default", "turn on the light of bedroom");

            //Console.WriteLine(result);


            Action action = () => Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            Parallel.Invoke(action, action, action, action, action);

            Console.ReadKey();

        }
    }
}
