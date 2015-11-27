using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.Tests
{
    [TestClass]
    public class AlternationTest
    {
        [TestMethod]
        public void Single_Alternation_Start()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ (happy|merry) christmas",
                              "- reply"});

            rs.sortReplies();

            var reply1 = rs.reply("default", "happy christmas");
            var reply2 = rs.reply("default", "merry christmas");


            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Single_Alternation_End()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ who (is your master|made you|created you)",
                              "- reply"});

            rs.sortReplies();

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
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ what is your (home|office) phone number",
                              "- reply"});

            rs.sortReplies();

            var reply1 = rs.reply("default", "what is your home phone number");
            var reply2 = rs.reply("default", "what is your office phone number");


            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Single_Alternation_Alone()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ (what is your name|who are you|who is this)",
                              "- reply"});
            rs.sortReplies();

            var reply1 = rs.reply("default", "what is your name");
            var reply2 = rs.reply("default", "who are you");
            var reply3 = rs.reply("default", "who is this");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
        }


        [TestMethod]
        public void Multiuple_Alternation_Alone()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ (happy|merry) (christmas|xmas)",
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
