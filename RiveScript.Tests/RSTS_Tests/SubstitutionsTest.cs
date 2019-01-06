using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class SubstitutionsTest
    {
        [TestMethod]
        public void RSTS_Substitutions__message_substitutions()
        {
            var rs = new RiveScript(debug: true);

            rs.stream(new[] { "+ whats up",
                              "- nm.",

                              "+ what is up",
                              "- Not much." });

            rs.sortReplies();

            rs.reply("whats up").AssertAreEqual("nm.");
            rs.reply("what's up?").AssertAreEqual("nm.");
            rs.reply("what is up?").AssertAreEqual("Not much.");


            rs.stream(new[] { "! sub whats  = what is",
                              "! sub what's = what is"});

            rs.sortReplies();

            rs.reply("whats up").AssertAreEqual("Not much.");
            rs.reply("what's up?").AssertAreEqual("Not much.");
            rs.reply("what is up?").AssertAreEqual("Not much.");
        }


        [TestMethod]
        public void RSTS_Substitutions__person_substitutions()
        {
            var rs = new RiveScript(debug: true);

            rs.stream(new[] { "+ say *",
                              "- <person>" });

            rs.sortReplies();

            rs.reply("say I am cool").AssertAreEqual("i am cool");
            rs.reply("say You are dumb").AssertAreEqual("you are dumb");
            

            rs.stream(new[] { "! person i am    = you are",
                              "! person you are = I am"});

            rs.sortReplies();

            rs.reply("say I am cool").AssertAreEqual("you are cool");
            rs.reply("say You are dumb").AssertAreEqual("I am dumb");

        }
    }
}