using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests
{
    /// <summary>
    /// Testes based on http://www.rivescript.com/try files
    /// </summary>
    [TestClass]
    public class SimpleReplyTest
    {
        [TestMethod]
        public void Single_Reply_Correct_Trigger_Should_Response_Correct_Reply()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ hello bot",
                              "- Hello human!" });
            rs.sortReplies();


            var reply = rs.reply("default", "hello bot");

            Assert.AreEqual(reply, "Hello human!");
        }

        [TestMethod]
        public void Single_Reply_Correct_Trigger_Should_Response_Correct_Reply_Ignoring_Comments()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "// This is the simple trigger again",
                              "+ hello bot // What the human says" ,
                              "- Hello human! // How the bot responds" });

            rs.sortReplies();


            var reply = rs.reply("default", "hello bot");

            Assert.AreEqual(reply, "Hello human!");
        }

        [TestMethod]
        public void Correct_Trigger_Should_Response_One_Of_Correct_Replies()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ hello bot",
                              "- Hello human!",
                              "- Hello!",
                              "- Hi there!",
                              "- Hey!",
                              "- Hi!" });

            rs.sortReplies();


            var reply = rs.reply("default", "hello bot");

            Assert.IsTrue(new[] { "Hello human!",
                                  "Hello!",
                                  "Hi there!",
                                  "Hey!",
                                  "Hi!" }.Contains(reply));
        }

        [TestMethod]
        public void Wheight_100_Shoud_Be_Only_Correct_Reply_Of_First()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ hello bot",
                              "- Hello human!{weight=100}",
                              "- Hello!" });

            rs.sortReplies();


            var reply = rs.reply("default", "hello bot");

            Assert.AreEqual("Hello human!", reply);
        }

        [TestMethod]
        public void Wheight_100_Shoud_Be_Only_Correct_Reply_Of_Second()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ hello bot",
                              "- Hello human!",
                              "- Hello!{weight=100}" });

            rs.sortReplies();

            var reply = rs.reply("default", "hello bot");

            Assert.AreEqual("Hello!", reply);
        }
    }
}
