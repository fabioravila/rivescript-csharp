using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests
{
    [TestClass]
    public class RandomReplyTest
    {
        [TestMethod]
        public void Simple_Random_Reply()
        {
            var rs = new RiveScriptEngine();

            rs.streamForTest(new[] { "+ hello bot",
                                     "- Hello",
                                     "- Hi" });

            var reply = rs.reply("hello bot");

            Assert.IsTrue(new[] { "Hello",
                                  "Hi" }.Contains(reply));
        }


        [TestMethod]
        public void Simple_Random_Reply_Alternative_Syntax()
        {
            var rs = new RiveScriptEngine();

            rs.streamForTest(new[] { "+ hello bot",
                                     "- {random}Hello|Hi|Hello human{/random}"});

            var reply = rs.reply("hello bot");

            Assert.IsTrue(new[] { "Hello",
                                  "Hi",
                                  "Hello human" }.Contains(reply));
        }

        [TestMethod]
        public void Simple_Random_Word_Start()
        {
            var rs = new RiveScriptEngine();

            rs.streamForTest(new[] { "+ hello bot",
                                     "- {random}Good|Best{/random} day"});

            var reply = rs.reply("hello bot");

            Assert.IsTrue(new[] { "Good day",
                                  "Best day" }.Contains(reply));
        }

        [TestMethod]
        public void Simple_Random_Word_Middle()
        {
            var rs = new RiveScriptEngine();

            rs.streamForTest(new[] { "+ hello bot",
                                     "- Hello {random}good|best{/random} day"});

            var reply = rs.reply("hello bot");

            Assert.IsTrue(new[] { "Hello good day",
                                  "Hello best day" }.Contains(reply));
        }

        [TestMethod]
        public void Simple_Random_Word_End()
        {
            var rs = new RiveScriptEngine();

            rs.streamForTest(new[] { "+ hello bot",
                                     "- Good {random}day|night{/random}"});

            var reply = rs.reply("hello bot");

            Assert.IsTrue(new[] { "Good day",
                                  "Good night" }.Contains(reply));
        }


        [TestMethod]
        public void Complex_Random_With_Conditionals()
        {
            var rs = new RiveScriptEngine();

            rs.streamForTest(new[] { "+ my name is *",
                                     "- <set name=<formal>>Nice to meet you, <get name>",
                                     "",
                                     "+ hello bot",
                                     "* <get name> != undefined => {random}",
                                     "^ Hello there, <get name>.|",
                                     "^ Hey, <get name>.{/random}",
                                     "- Hello there!",
                                     "- Hi there",
                                    });

            //Without name

            rs.reply("hello bot")
              .AssertContains("Hello there!", "Hi there");

            //Set name
            rs.reply("my name is John")
              .AssertAreEqual("Nice to meet you, John");


            rs.reply("hello bot")
               .AssertContains("Hello there, John.", "Hey, John.");
        }


        


        [TestMethod]
        public void Complex_Random_With_Conditionals_And_Arrays_Triggers()
        {
            var rs = new RiveScriptEngine();

            rs.streamForTest(new[] { "! array hello = hi hello",
                                     "^ hi there | good day",
                                     "",
                                     "+ my name is *",
                                     "- <set name=<formal>>Nice to meet you, <get name>",
                                     "",
                                     "+ @hello",
                                     "* <get name> != undefined => {random}",
                                     "^ Hello there, <get name>.|",
                                     "^ Hey, <get name>.{/random}",
                                     "- Hello there!",
                                     "- Hi there",
                                    });

            //Without name

            rs.reply("hello")
              .AssertContains("Hello there!", "Hi there");

            //Set name
            rs.reply("my name is John")
              .AssertAreEqual("Nice to meet you, John");


            rs.reply("hi")
               .AssertContains("Hello there, John.", "Hey, John.");
        }
    }
}
