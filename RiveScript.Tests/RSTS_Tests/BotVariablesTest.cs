using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class BotVariablesTest
    {
        [TestMethod]
        public void RSTS_BotVariables__bot_variables()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {    "! var name = Aiden",
                                                            "! var age = 5",
                                                            "+ what is your name",
                                                            "- My name is <bot name>.",

                                                            "+ how old are you",
                                                            "- I am <bot age>.",

                                                            "+ what are you",
                                                            "- I'm <bot gender>.",

                                                            "+ happy birthday",
                                                            "- <bot age=6>Thanks!" });

            rs.reply("What is your name?").AssertAreEqual("My name is Aiden.");
            rs.reply("How old are you?").AssertAreEqual("I am 5.");
            rs.reply("What are you?").AssertAreEqual("I'm undefined.");
            rs.reply("Happy birthday!").AssertAreEqual("Thanks!");
            rs.reply("How old are you?").AssertAreEqual("I am 6.");
        }


        [TestMethod]
        public void RSTS_BotVariables__global_variables()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {   "! global debug = false",
                                                           "+ debug mode",
                                                           "- Debug mode is: <env debug>",

                                                           "+ set debug mode *",
                                                           "- <env debug=<star>>Switched to <star>." });

            rs.reply("Debug mode.").AssertAreEqual("Debug mode is: false");
            rs.reply("Set debug mode true").AssertAreEqual("Switched to true.");
            rs.reply("Debug mode?").AssertAreEqual("Debug mode is: true");
        }
    }
}