using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class MathTest
    {
        [TestMethod]
        public void RSTS_Math__addition()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "+ test counter",
                                                         "- <set counter=0>counter set",

                                                         "+ show",
                                                         "- counter = <get counter>",

                                                         "+ add",
                                                         "- <add counter=1>adding",

                                                         "+ sub",
                                                         "- <sub counter=1>subbing",

                                                         "+ div",
                                                         "- <set counter=10>",
                                                         "^ <div counter=2>",
                                                         "^ divving",

                                                         "+ mult",
                                                         "- <set counter=10>",
                                                         "^ <mult counter=2>",
                                                         "^ multing"});

            rs.reply("test counter").AssertAreEqual("counter set");
            rs.reply("show").AssertAreEqual("counter = 0");
            rs.reply("add").AssertAreEqual("adding");
            rs.reply("show").AssertAreEqual("counter = 1");
            rs.reply("sub").AssertAreEqual("subbing");
            rs.reply("show").AssertAreEqual("counter = 0");
            rs.reply("div").AssertAreEqual("divving");
            rs.reply("show").AssertAreEqual("counter = 5");
            rs.reply("mult").AssertAreEqual("multing");
            rs.reply("show").AssertAreEqual("counter = 20");

        }
    }
}