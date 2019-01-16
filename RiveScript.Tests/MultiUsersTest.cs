using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class MultiUsersTest
    {
        private const string USER_1 = "USER_1";
        private const string USER_2 = "USER_2";

        [TestMethod]
        public void Previous_Replies()
        {
            var rs = new RiveScriptEngine(Config.Debug);

            rs.stream(@"! sub who's  = who is
                        ! sub it's   = it is
                        ! sub didn't = did not


                        + knock knock
                        - Who's there?

                        + *
                        % who is there
                        - <sentence> who?

                        + *
                        % * who
                        - Haha! <sentence>!

                        + *
                        - I don't know.");

            rs.sortReplies();


            rs.reply(USER_1, "knock knock").AssertAreEqual("Who's there?");

            //rs.reply(USER_2, "Canoe").AssertAreEqual("I don't know.");

            rs.reply(USER_1, "Canoe").AssertAreEqual("Canoe who?");
            rs.reply(USER_1, "Canoe reply").AssertAreEqual("Haha! Canoe reply!");

            //rs.reply(USER_2, "Canoe reply").AssertAreEqual("I don't know.");
            //rs.reply(USER_2, "knock knock").AssertAreEqual("Who's there?");

            rs.reply(USER_1, "Canoe").AssertAreEqual("I don't know.");
            //rs.reply(USER_2, "Canoe").AssertAreEqual("Canoe who?");
            rs.reply(USER_1, "Canoe reply").AssertAreEqual("I don't know.");
        }
    }
}
