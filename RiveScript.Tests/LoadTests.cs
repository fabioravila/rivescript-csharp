using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    [DeploymentItem("TestData/basic_reply_1.rs", "TestData")]
    [DeploymentItem("TestData/basic_reply_2.rive", "TestData")]
    [DeploymentItem("TestData/basic_reply_3.txt", "TestData")]
    [DeploymentItem("TestData/InnerDir/basic_reply_4.rs", "TestData/InnerDir")]
    public class LoadTests
    {
        [TestMethod]
        public void Load_Simple_File_Commom_Extension()
        {
            var rs = new RiveScript(true);

            var result = rs.loadFile("TestData/basic_reply_1.rs");
            Assert.IsTrue(result);

            rs.sortReplies();

            rs.reply("trigger1").AssertAreEqual("reply1");
        }

        [TestMethod]
        public void Load_Simple_File_Commom_Extension_2()
        {
            var rs = new RiveScript(true);

            var result = rs.loadFile("TestData/basic_reply_2.rive");
            Assert.IsTrue(result);

            rs.sortReplies();

            rs.reply("trigger2").AssertAreEqual("reply2");
        }

        [TestMethod]
        public void Load_Simple_File_Non_Common_Extension()
        {
            var rs = new RiveScript(true);

            var result = rs.loadFile("TestData/basic_reply_3.txt");
            Assert.IsTrue(result);

            rs.sortReplies();

            rs.reply("trigger3").AssertAreEqual("reply3");
        }

        [TestMethod]
        public void Load_All_Directory_No_Extension_Specify()
        {
            var rs = new RiveScript(true);

            var result = rs.loadDirectory("TestData");
            Assert.IsTrue(result);

            rs.sortReplies();

            rs.reply("trigger1").AssertAreEqual("reply1");
            rs.reply("trigger2").AssertAreEqual("reply2");
            rs.reply("trigger4").AssertAreEqual("reply4");//Inner folder
            rs.reply("trigger3").AssertAreNotEqual("reply3"); //Non-common extension
        }

        [TestMethod]
        public void Load_All_Directory_With_Extension_Specify()
        {
            var rs = new RiveScript(true);

            var result = rs.loadDirectory("TestData", new[] { ".rs" });
            Assert.IsTrue(result);

            rs.sortReplies();

            rs.reply("trigger1").AssertAreEqual("reply1");//.rs
            rs.reply("trigger2").AssertAreNotEqual("reply2"); //.rive
            rs.reply("trigger3").AssertAreNotEqual("reply3"); //.txt
            rs.reply("trigger4").AssertAreEqual("reply4");//.rs

        }

        [TestMethod]
        public void Load_Stream_Single_String()
        {
            var rs = new RiveScript(true);

            var result = rs.stream("+ hello bot\n - Hello human!");
            Assert.IsTrue(result);

            rs.sortReplies();

            rs.reply("hello bot").AssertAreEqual("Hello human!");
        }
    }
}
