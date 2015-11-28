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
            var rs = TestHelper.getStreamed(new[] { "+ [i] do not have friends",
                                                    "- reply"});

            var reply1 = rs.reply("default", "i do not have friends");
            var reply2 = rs.reply("default", "i do not have friends");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Single_Optional_End()
        {
            var rs = TestHelper.getStreamed(new[] { "+ i do not have friends [here]",
                                                    "- reply"});

            var reply1 = rs.reply("default", "i do not have friends here");
            var reply2 = rs.reply("default", "i do not have friends");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Single_Optional_Middle()
        {
            var rs = TestHelper.getStreamed(new[] { "+ i do not have [any] friends",
                                                    "- reply"});

            var reply1 = rs.reply("default", "i do not have any friends");
            var reply2 = rs.reply("default", "i do not have friends");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
        }

        [TestMethod]
        public void Multiple_Optional_Start()
        {
            var rs = TestHelper.getStreamed(new[] { "+ [am] [i] boy or girl",
                                                      "- reply"});

            var reply1 = rs.reply("default", "am i boy or girl");
            var reply2 = rs.reply("default", "i boy or girl");
            var reply3 = rs.reply("default", "am boy or girl");
            var reply4 = rs.reply("default", "boy or girl");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);

        }

        [TestMethod]
        public void Multiple_Optional_Middle()
        {
            var rs = TestHelper.getStreamed(new[] { "+ am i [a] boy or [a] girl",
                                                    "- reply"});

            var reply1 = rs.reply("default", "am i a boy or a girl");
            var reply2 = rs.reply("default", "am i boy or a girl");
            var reply3 = rs.reply("default", "am i a boy or girl");
            var reply4 = rs.reply("default", "am i boy or girl");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);

        }

        [TestMethod]
        public void Multiple_Optional_End()
        {

            var rs = TestHelper.getStreamed(new[] { "+ am i boy [or] [girl]",
                                                    "- reply"});

            var reply1 = rs.reply("default", "am i boy or girl");
            var reply2 = rs.reply("default", "am i boy girl");
            var reply3 = rs.reply("default", "am i boy or");
            var reply4 = rs.reply("default", "am i boy");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);

        }

        [TestMethod]
        public void Multiple_Optional_Start_Middle()
        {
            var rs = TestHelper.getStreamed(new[] { "+ [am] i [boy] girl",
                                                    "- reply"});


            var reply1 = rs.reply("default", "am i boy girl");
            var reply2 = rs.reply("default", "i boy girl");
            var reply3 = rs.reply("default", "am i girl");
            var reply4 = rs.reply("default", "i girl");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);
        }

        [TestMethod]
        public void Multiple_Optional_Middle_End()
        {
            var rs = TestHelper.getStreamed(new[] { "+ am i [boy] or [girl]",
                                                    "- reply"});


            var reply1 = rs.reply("default", "am i boy or girl");
            var reply2 = rs.reply("default", "am i or girl");
            var reply3 = rs.reply("default", "am i boy or");
            var reply4 = rs.reply("default", "am i or");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);
        }

        [TestMethod]
        public void Multiple_Optional_Start_End()
        {
            var rs = TestHelper.getStreamed(new[] { "+ [am] i boy [girl]",
                                                    "- reply"});


            var reply1 = rs.reply("default", "am i boy girl");
            var reply2 = rs.reply("default", "i boy girl");
            var reply3 = rs.reply("default", "am i boy");
            var reply4 = rs.reply("default", "i boy");

            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);

        }

        [TestMethod]
        public void Multiple_Optional_Start_Middle_End()
        {
            var rs = TestHelper.getStreamed(new[] { "+ [am] i [boy] or [girl]",
                                                    "- reply"});


            var reply1 = rs.reply("default", "am i boy or girl");
            var reply2 = rs.reply("default", "i boy or girl");
            var reply3 = rs.reply("default", "am i or girl");
            var reply4 = rs.reply("default", "am i boy or");
            var reply5 = rs.reply("default", "i or girl");
            var reply6 = rs.reply("default", "am i or");
            var reply7 = rs.reply("default", "i boy or");
            var reply8 = rs.reply("default", "i or");


            Assert.AreEqual("reply", reply1);
            Assert.AreEqual("reply", reply2);
            Assert.AreEqual("reply", reply3);
            Assert.AreEqual("reply", reply4);
            Assert.AreEqual("reply", reply5);
            Assert.AreEqual("reply", reply6);
            Assert.AreEqual("reply", reply7);
            Assert.AreEqual("reply", reply8);

        }

        [TestMethod]
        public void Optional_With_Alternations()
        {
            var rs = TestHelper.getStreamed(new[] { "+ tell me your [home|office|work|cell] [phone] number",
                                                    "- reply"});

            var r1 = rs.reply("default", "Tell me your phone number");
            var r2 = rs.reply("default", "Tell me your number");
            var r3 = rs.reply("default", "Tell me your home phone number");
            var r4 = rs.reply("default", "Tell me your home number");
            var r5 = rs.reply("default", "Tell me your office phone number");
            var r6 = rs.reply("default", "Tell me your office number");
            var r7 = rs.reply("default", "Tell me your work phone number");
            var r8 = rs.reply("default", "Tell me your work number");
            var r9 = rs.reply("default", "Tell me your cell phone number");
            var r10 = rs.reply("default", "Tell me your cell number");


            Assert.AreEqual("reply", r1);
            Assert.AreEqual("reply", r2);
            Assert.AreEqual("reply", r3);
            Assert.AreEqual("reply", r4);
            Assert.AreEqual("reply", r5);
            Assert.AreEqual("reply", r6);
            Assert.AreEqual("reply", r7);
            Assert.AreEqual("reply", r8);
            Assert.AreEqual("reply", r9);
            Assert.AreEqual("reply", r10);
        }
    }
}
