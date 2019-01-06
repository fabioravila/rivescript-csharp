using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class PublicIssuesTest
    {
        /*
        * Basead on https://github.com/aichaos/rivescript-js/issues/48
        */
        [TestMethod]
        public void Alternation_With_Optional_And_Space_Between()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ (aa|bb|cc) [bogus]",
                              "- reply"
            });

            rs.sortReplies();

            rs.reply("aa bogus").AssertAreEqual("reply");
            rs.reply("aabogus").AssertAreNotEqual("reply");
        }
    }
}
