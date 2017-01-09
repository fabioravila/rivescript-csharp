using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class BeginTest
    {
        [TestMethod]
        public void RSTS_Begin__no_begin_block()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "+ hello bot",
                                                         "- Hello human." });

            rs.reply("hello bot").AssertAreEqual("Hello human.");
        }


        [TestMethod]
        public void RSTS_Begin__simple_begin_block()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "> begin",
                                                         "  + request",
                                                         "  - {ok}",
                                                         "< begin",

                                                         "+ hello bot",
                                                         "- Hello human." });

            rs.reply("Hello bot.").AssertAreEqual("Hello human.");
        }


        [TestMethod]
        public void RSTS_Begin__blocked_begin_block()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "> begin",
                                                         "  + request",
                                                         "  - Nope.",
                                                         "< begin",

                                                         "+ hello bot",
                                                         "- Hello human." });

            rs.reply("Hello bot.").AssertAreEqual("Nope.");
        }


        [TestMethod]
        public void RSTS_Begin__conditional_begin_block()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "	> begin",
                                                         "		+ request",
                                                         "		* <get met> == undefined => <set met=true>{ok}",
                                                         "		* <get name> != undefined => <get name>: {ok}",
                                                         "		- {ok}",
                                                         "	< begin",
                                                         "	+ hello bot",
                                                         "	- Hello human.",

                                                         "	+ my name is *",
                                                         "	- <set name=<formal>>Hello, <get name>." });

            rs.reply("Hello bot.").AssertAreEqual("Hello human.");

            rs.setUservar("met", "true");
            rs.setUservar("name", "undefined");

            rs.reply("My name is bob").AssertAreEqual("Hello, Bob.");
            rs.getUserVar("name").AssertAreEqual("Bob");

            rs.reply("Hello bot").AssertAreEqual("Bob: Hello human.");
        }







    }
}