using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests.RSTS
{
    /// <summary>
    /// Testes based on http://www.rivescript.com/try files
    /// </summary>
    [TestClass]
    public class Replies
    {
        [TestMethod]
        public void RSTS_Replies__reply_arrays()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {
                                                    "! array greek = alpha beta gamma",
                                                    "! array test = testing trying",
                                                    "! array format = <uppercase>|<lowercase>|<formal>|<sentence>",
                                                    "+ test random array",
                                                    "- Testing (@greek) array.",

                                                    "+ test two random arrays",
                                                    "- {formal}(@test){/formal} another (@greek) array.",

                                                    "+ test nonexistant array",
                                                    "- This (@array) does not exist.",

                                                    "+ test more arrays",
                                                    "- I'm (@test) more (@greek) (@arrays).",

                                                    "+ test weird syntax",
                                                    "- This (@ greek) shouldn't work, and neither should this @test.",

                                                    "+ random format *",
                                                    "- (@format)" });

            TestHelper.Loop(10, () =>
            {

                rs.reply("test random array")
                  .AssertContains("Testing alpha array.",
                                  "Testing beta array.",
                                  "Testing gamma array.");


                rs.reply("test two random arrays")
                  .AssertContains("Testing another alpha array.",
                                  "Testing another beta array.",
                                  "Testing another gamma array.",
                                  "Trying another alpha array.",
                                  "Trying another beta array.",
                                  "Trying another gamma array.");


                rs.reply("test nonexistant array")
                  .AssertAreEqual("This (@array) does not exist.");

                rs.reply("test more arrays")
                  .AssertContains("I'm testing more alpha (@arrays).",
                                  "I'm testing more beta (@arrays).",
                                  "I'm testing more gamma (@arrays).",
                                  "I'm trying more alpha (@arrays).",
                                  "I'm trying more beta (@arrays).",
                                  "I'm trying more gamma (@arrays).");

                rs.reply("test weird syntax")
                  .AssertAreEqual("This (@ greek) shouldn't work, and neither should this @test.");


                rs.reply("random format hello world")
                  .AssertContains("HELLO WORLD",
                                  "hello world",
                                  "Hello World",
                                  "Hello world");

            });
        }


        [TestMethod]
        public void RSTS_Replies__embedded_tags()
        {

            var rs = TestHelper.getEmptyStreamed(new[] { " + my name is *",
                                                         "* <get name> != undefined => <set oldname=<get name>>I thought\\s",
                                                         "  ^ your name was <get oldname>?",
                                                         "  ^ <set name=<formal>>",
                                                         "- <set name=<formal>>OK.",

                                                         "+ what is my name",
                                                         "- Your name is <get name>, right?",

                                                         "+ html test",
                                                         "- <set name=<b>Name</b>>This has some non-RS <em>tags</em> in it."});


            rs.reply("What is my name?").AssertAreEqual("Your name is undefined, right?");
            rs.reply("My name is Alice.").AssertAreEqual("OK.");
            rs.reply("My name is Bob.").AssertAreEqual("I thought your name was Alice?");
            rs.reply("What is my name?").AssertAreEqual("Your name is Bob, right?");
            rs.reply("HTML Test").AssertAreEqual("This has some non-RS <em>tags</em> in it.");
        }
    }
}