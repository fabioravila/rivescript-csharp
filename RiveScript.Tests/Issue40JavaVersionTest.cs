using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiveScript.Macro;

namespace RiveScript.Tests
{
    [TestClass]
    public class Issue40JavaVersionTest
    {
        private const string USER = "default";

        [TestMethod]
        public void Quote_String_Arg_Whithout_Of_Space_ShoudWork_As_multiple()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "! version = 2.0"
                                                       , "+ *"
                                                       , "- <call>f <get v></call>"
            });

            rs.setDebug(true);
            rs.setSubroutine("f", new F());
            rs.setUservar(USER, "v", "a b");
            rs.sortReplies();

            var reply = rs.reply(USER, "Hi");
            reply.AssertAreEqual("a,b");
        }


        [TestMethod]
        public void Quote_String_Arg_With_Of_Space_ShoudWork_As_One()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "! version = 2.0"
                                                       , "+ *"
                                                       , "- <call>f \"<get v>\"</call>"
            });
            var user_var = "a b";

            rs.setDebug(true);
            rs.setSubroutine("f", new F());
            rs.setUservar(USER, "v", user_var);
            rs.sortReplies();

            var reply = rs.reply(USER, "Hi");
            reply.AssertAreEqual(user_var);
        }

        [TestMethod]
        public void Quote_String_Arg_Regardless_ShoudWork_As_Expected()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "! version = 2.0"
                                                       , "+ *"
                                                       , "- <call>f \"a b\" c d</call>"
            });

            rs.setDebug(true);
            rs.setSubroutine("f", new F());
            rs.sortReplies();

            var reply = rs.reply(USER, "Hi");
            reply.AssertAreEqual("a b,c,d");
        }


        public class F : ISubroutine
        {
            public string Call(RiveScriptEngine rs, string[] args)
            {
                return string.Join(",", args);
            }
        }
    }
}
