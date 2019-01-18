using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class AlternationTest
    {
        [TestMethod]
        public void Single_Alternation_Start()
        {
            var rs = TestHelper.getStreamed(new[] { "+ (happy|merry) christmas",
                                                    "- reply"});


            var reply1 = rs.reply("default", "happy christmas");
            var reply2 = rs.reply("default", "merry christmas");


            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Single_Alternation_End()
        {
            var rs = TestHelper.getStreamed(new[] { "+ who (is your master|made you|created you)",
                                                    "- reply"});

            var reply1 = rs.reply("default", "who is your master");
            var reply2 = rs.reply("default", "who made you");
            var reply3 = rs.reply("default", "who created you");


            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
        }

        [TestMethod]
        public void Single_Alternation_Middle()
        {
            var rs = TestHelper.getStreamed(new[] { "+ what is your (home|office) phone number",
                                                    "- reply"});

            var reply1 = rs.reply("default", "what is your home phone number");
            var reply2 = rs.reply("default", "what is your office phone number");


            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Single_Alternation_Alone()
        {
            var rs = TestHelper.getStreamed(new[] { "+ (what is your name|who are you|who is this)",
                                                    "- reply"});

            var reply1 = rs.reply("default", "what is your name");
            var reply2 = rs.reply("default", "who are you");
            var reply3 = rs.reply("default", "who is this");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
        }

        [TestMethod]
        public void Multiple_Alternation_Alone()
        {
            var rs = TestHelper.getStreamed(new[] { "+ (happy|merry) (christmas|xmas)",
                                                     "- reply"});
            rs.sortReplies();

            var reply1 = rs.reply("default", "happy christmas");
            var reply2 = rs.reply("default", "happy xmas");
            var reply3 = rs.reply("default", "merry christmas");
            var reply4 = rs.reply("default", "merry xmas");


            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);
        }
    }
}
