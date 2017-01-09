using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class TriggersTest
    {
        [TestMethod]
        public void RSTS_Triggers__weighted_triggers()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {
                                                            "+ * or something{weight=10}",
                                                            "- Or something. <@>",

                                                            "+ can you run a google search for *",
                                                            "- Sure!",

                                                            "+ hello *{weight=20}",
                                                            "- Hi there!",

                                                            "// Test that spaces before or after the {weight} tag are gobbled up along",
                                                            "// with the {weight} tag itself.",

                                                            "+ something{weight=100}",
                                                            "- Weighted something",

                                                            "+ something",
                                                            "- Unweighted something",

                                                            "+ nothing {weight=100}",
                                                            "- Weighted nothing",

                                                            "+ nothing",
                                                            "- Unweighted nothing",

                                                            "+ {weight=100}everything",
                                                            "- Weighted everything",

                                                            "+ everything",
                                                            "- Unweighted everything",

                                                            "+ {weight=100}   blank",
                                                            "- Weighted blank",

                                                            "+ blank",
                                                            "- Unweighted blank" });

            rs.reply("Hello robot.").AssertAreEqual("Hi there!");
            rs.reply("Hello or something.").AssertAreEqual("Hi there!");
            rs.reply("Can you run a Google search for Node?").AssertAreEqual("Sure!");
            rs.reply("Can you run a Google search for Node or something?").AssertAreEqual("Or something. Sure!");
            rs.reply("something").AssertAreEqual("Weighted something");
            rs.reply("nothing").AssertAreEqual("Weighted nothing");
            rs.reply("everything").AssertAreEqual("Weighted everything");
            rs.reply("blank").AssertAreEqual("Weighted blank");
        }
    }
}