using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class InheritsIncludesTest
    {
        private RiveScript getWithGlobalTopic(string[] code)
        {
            var rs = new RiveScript(true);
            rs.stream(new[] { "+ go topic *",
                              "- {topic=<star>}ok",
                              "> topic global",
                              "	+ global trigger",
                              "	- global reply",
                              "< topic",
                              "",
                              "> topic global1",
                              "	+ *",
                              "	- global1 reply",
                              "< topic",
                              ""});


            rs.stream(code);
            rs.sortReplies();
            return rs;
        }

        [TestMethod]
        public void Alone_Topic_Redirection()
        {
            var rs = getWithGlobalTopic(new[] { "" });


            rs.reply("go topic global")
                .AssertAreEqual("ok");

            rs.reply("global trigger")
                .AssertAreEqual("global reply");
        }

        [TestMethod]
        public void Simple_Include()
        {
            var rs = getWithGlobalTopic(new[] { "",
                                                "> topic a includes global",
                                                "+ a trigger",
                                                "- a reply",
                                                "" });


            rs.reply("go topic a")
                .AssertAreEqual("ok");

            rs.reply("a trigger")
                .AssertAreEqual("a reply");

            rs.reply("global trigger")
                .AssertAreEqual("global reply");
        }

        [TestMethod]
        public void Simple_Include_With_No_Override()
        {
            var rs = getWithGlobalTopic(new[] { "",
                                                "> topic a include global1",
                                                "+ a trigger",
                                                "- a reply",
                                                "" });


            rs.reply("go topic a")
                .AssertAreEqual("ok");

            rs.reply("a trigger")
                .AssertAreEqual("a reply");

            rs.reply("something")
                .AssertAreEqual("ERR: No Reply Matched");

            //Low priority
            rs.reply("global1 trigger")
                .AssertAreEqual("ERR: No Reply Matched");
        }

        [TestMethod]
        public void Simple_Inherits()
        {
            var rs = getWithGlobalTopic(new[] { "",
                                                "> topic a inherits global",
                                                "+ a trigger",
                                                "- a reply",
                                                "" });


            rs.reply("go topic a")
                .AssertAreEqual("ok");

            rs.reply("a trigger")
                .AssertAreEqual("a reply");

            //Low priority
            rs.reply("global trigger")
                .AssertAreEqual("global reply");
        }

        [TestMethod]
        public void Simple_Inherits_With_Override()
        {
            var rs = getWithGlobalTopic(new[] { "",
                                                "> topic a inherits global1",
                                                "+ a trigger",
                                                "- a reply",
                                                "" });


            rs.reply("go topic a")
                .AssertAreEqual("ok");

            rs.reply("a trigger")
                .AssertAreEqual("a reply");

            rs.reply("global1 trigger")
                .AssertAreEqual("global1 reply");

            rs.reply("something")
              .AssertAreEqual("global1 reply");
        }

    }
}
