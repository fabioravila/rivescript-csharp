using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class OptionalTest
    {
        [TestMethod]
        public void Single_Optional_Start()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ [i] do not have friends",
                              "- reply"});

            rs.sortReplies();

            var reply1 = rs.reply("default", "i do not have friends");
            var reply2 = rs.reply("default", "i do not have friends");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Single_Optional_End()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ i do not have friends [here]",
                              "- reply"});

            rs.sortReplies();

            var reply1 = rs.reply("default", "i do not have friends here");
            var reply2 = rs.reply("default", "i do not have friends");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Single_Optional_Middle()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ i do not have [any] friends",
                              "- reply"});

            rs.sortReplies();

            var reply1 = rs.reply("default", "i do not have any friends");
            var reply2 = rs.reply("default", "i do not have friends");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Multiple_Optional_Middle()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ am i [a] boy or [a] girl",
                              "- reply"});

            rs.sortReplies();

            var reply1 = rs.reply("default", "am i a boy or a girl");
            var reply2 = rs.reply("default", "am i boy or a girl");
            var reply3 = rs.reply("default", "am i a boy or girl");
            var reply4 = rs.reply("default", "am i boy or girl");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);

        }
    }
}
