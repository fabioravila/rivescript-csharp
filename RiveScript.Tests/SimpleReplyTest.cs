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
            var rs = TestHelper.getStreamed(new[] { "+ hello bot",
                                                    "- Hello human!" });

            var reply = rs.reply("default", "hello bot");

            Assert.AreEqual(reply, "Hello human!");
        }

        [TestMethod]
        public void Single_Reply_Correct_Trigger_Should_Response_Correct_Reply_Ignoring_Comments()
        {
            var rs = TestHelper.getStreamed(new[] { "// This is the simple trigger again",
                                                    "+ hello bot // What the human says" ,
                                                    "- Hello human! // How the bot responds" });

            var reply = rs.reply("default", "hello bot");

            Assert.AreEqual(reply, "Hello human!");
        }

        [TestMethod]
        public void Correct_Trigger_Should_Response_One_Of_Correct_Replies()
        {
            var rs = TestHelper.getStreamed(new[] { "+ hello bot",
                                                    "- Hello human!",
                                                    "- Hello!",
                                                    "- Hi there!",
                                                    "- Hey!",
                                                    "- Hi!" });

            var reply = rs.reply("default", "hello bot");

            Assert.IsTrue(new[] { "Hello human!",
                                  "Hello!",
                                  "Hi there!",
                                  "Hey!",
                                  "Hi!" }.Contains(reply));
        }

        [TestMethod]
        public void Wheight_1000_Shoud_Be_Only_Correct_Reply_Of_First()
        {
            var rs = TestHelper.getStreamed(new[] { "+ hello bot",
                                                    "- Hello human!{weight=1000}",
                                                    "- Hello!" });

            var reply = rs.reply("default", "hello bot");

            Assert.AreEqual("Hello human!", reply);
        }

        [TestMethod]
        public void Wheight_1000_Shoud_Be_Only_Correct_Reply_Of_Second()
        {
            var rs = TestHelper.getStreamed(new[] { "+ hello bot",
                                                    "- Hello human!",
                                                    "- Hello!{weight=1000}" });

            var reply = rs.reply("default", "hello bot");

            Assert.AreEqual("Hello!", reply);
        }
    }
}
