using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class RepetitionTest
    {
        [TestMethod]
        public void Last_Reply_Repetition()
        {
            var rs = new RiveScriptEngine(Config.Default);

            rs.streamForTest(new[] { "+ <reply>",
                                     "- I just say that",
                                     "+ hi",
                                     "- hello"

            });

            var r1 = rs.reply("hi");
            Assert.AreEqual("hello", r1);

            rs.reply("hello")
              .AssertAreEqual("I just say that");
        }


        [TestMethod]
        public void Last_Input_Repetition()
        {
            var rs = new RiveScriptEngine(Config.Default);

            rs.streamForTest(new[] { "+ <input>",
                                     "- Didn't you just say that?",
                                     "+ hi",
                                     "- Hello"

            });

            rs.reply("Hi")
              .AssertAreEqual("Hello");

            rs.reply("Hi")
              .AssertAreEqual("Didn't you just say that?");
        }



        [TestMethod]
        public void Last_Reply_Repetition_UTF8()
        {
            var rs = new RiveScriptEngine(Config.UTF8);

            rs.streamForTest(new[] { "+ <reply>",
                                     "- Eu não acabei de falar isso?",
                                     "+ olá",
                                     "- Olá como vai"

            });

            var r1 = rs.reply("Olá");
            Assert.AreEqual("Olá como vai", r1);

            rs.reply("Olá como vai")
              .AssertAreEqual("Eu não acabei de falar isso?");
        }


        [TestMethod]
        public void Last_Input_Repetition_UTF8()
        {
            var rs = new RiveScriptEngine(Config.UTF8);

            rs.streamForTest(new[] { "+ <input>",
                                     "- Você não acabou de falar isso?",
                                     "+ olá",
                                     "- Oi"

            });

            rs.reply("Olá")
              .AssertAreEqual("Oi");

            rs.reply("Olá")
              .AssertAreEqual("Você não acabou de falar isso?");
        }


    }
}
