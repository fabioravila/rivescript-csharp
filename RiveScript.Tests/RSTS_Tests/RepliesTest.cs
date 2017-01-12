using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class RepliesTest
    {
        [TestMethod]
        public void RSTS_Replies__previous()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {"! sub who's  = who is",
                                                        "! sub it's   = it is",
                                                        "! sub didn't = did not",
                                                        "+ knock knock",
                                                        "- Who's there?",

                                                        "+ *",
                                                        "% who is there",
                                                        "- <sentence> who?",

                                                        "+ *",
                                                        "% * who",
                                                        "- Haha! <sentence>!",

                                                        "+ *",
                                                        "- I don't know." });

            rs.reply("knock knock").AssertAreEqual("Who's there?");
            rs.reply("Canoe").AssertAreEqual("Canoe who?");
            rs.reply("Canoe help me with my homework?").AssertAreEqual("Haha! Canoe help me with my homework!");
            rs.reply("Hello").AssertAreEqual("I don't know.");
        }

        [TestMethod]
        public void RSTS_Replies__random()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {"+ test random response",
                                                        "- One.",
                                                        "- Two.",

                                                        "+ test random tag",
                                                        "- This sentence has a random {random}word|bit{/random}." });

            TestHelper.Loop(5, () =>
            {
                rs.reply("Test random response").AssertContains("One.", "Two.");
                rs.reply("Test random tag").AssertContains("This sentence has a random word.",
                                                           "This sentence has a random bit.");
            });
        }

        [TestMethod]
        public void RSTS_Replies__continuations()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {"+ tell me a poem",
                                                        "- There once was a man named Tim,\\s",
                                                        "^ who never quite learned how to swim.\\s",
                                                        "^ He fell off a dock, and sank like a rock,\\s",
                                                        "^ and that was the end of him." });

            rs.reply("Tell me a poem.").AssertAreEqual("There once was a man named Tim, who never quite learned how to swim. He fell off a dock, and sank like a rock, and that was the end of him.");
        }

        [TestMethod]
        public void RSTS_Replies__redirects()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {"+ hello",
                                                        "- Hi there!",

                                                        "+ hey",
                                                        "@ hello",

                                                        "// Test the {@} tag with and without spaces.",
                                                        "+ hi there",
                                                        "- {@hello}",

                                                        "+ howdy",
                                                        "- {@ hello}",

                                                        "+ hola",
                                                        "- {@ hello }" });

            rs.reply("hello").AssertAreEqual("Hi there!");
            rs.reply("hey").AssertAreEqual("Hi there!");
            rs.reply("hi there").AssertAreEqual("Hi there!");
            rs.reply("howdy").AssertAreEqual("Hi there!");
            rs.reply("hola").AssertAreEqual("Hi there!");
        }

        [TestMethod]
        public void RSTS_Replies__redirect_with_undefined_input()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {"+ test",
                                                        "- {topic=test}{@hi}",

                                                        "+ test *",
                                                        "- {topic=test}<@>",

                                                        "> topic test",
                                                        "  + hi",
                                                        "  - hello",

                                                        "  + *",
                                                        "  - {topic=random}<@>",
                                                        "< topic",

                                                        "+ *",
                                                        "- Wildcard \"<star>\"!" });

            rs.reply("test").AssertAreEqual("hello");
            rs.reply("test x").AssertAreEqual("Wildcard \"x\"!");
            rs.reply("?").AssertAreEqual("Wildcard \"\"!");
        }

        [TestMethod]
        public void RSTS_Replies__redirect_with_undefined_vars()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {"! var globaltest = set test name test",
                                                        "+ test",
                                                        "- {topic=test}{@<get test_name>}",

                                                        "+ test without redirect",
                                                        "- {topic=test}<get test_name>",

                                                        "+ set test name *",
                                                        "- <set test_name=<star>>{@test}",

                                                        "+ get global test",
                                                        "@ <bot globaltest>",

                                                        "+ get bad global test",
                                                        "@ <bot badglobaltest>",

                                                        "> topic test",
                                                        "  + test",
                                                        "  - hello <get test_name>!{topic=random}",

                                                        "  + *",
                                                        "  - {topic=random}<@>",
                                                        "< topic",

                                                        "+ *",
                                                        "- Wildcard \"<star>\"!" });

            // No variable set, should go through wildcard.
            rs.reply("test").AssertAreEqual("Wildcard \"undefined\"!");
            rs.reply("test without redirect").AssertAreEqual("undefined");

            //Variable set, should respond with text
            rs.reply("set test name test").AssertAreEqual("hello test!");

            //Different variable set, should get wildcard
            rs.reply("set test name newtest").AssertAreEqual("Wildcard \"newtest\"!");

            //Test redirects using bot variable
            rs.reply("get global test").AssertAreEqual("hello test!");
            rs.reply("get bad global test").AssertAreEqual("Wildcard \"undefined\"!");
        }

        [TestMethod]
        public void RSTS_Replies__conditions()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {"+ i am # years old",
                                                        "- <set age=<star>>OK.",

                                                        "+ what can i do",
                                                        "* <get age> == undefined => I don't know.",
                                                        "* <get age> >  25 => Anything you want.",
                                                        "* <get age> == 25 => Rent a car for cheap.",
                                                        "* <get age> >= 21 => Drink.",
                                                        "* <get age> >= 18 => Vote.",
                                                        "* <get age> <  18 => Not much of anything.",

                                                        "+ am i your master",
                                                        "* <get master> == true => Yes.",
                                                        "- No." });

            rs.reply("What can I do?").AssertAreEqual("I don't know.");
            rs.reply("I am 16 years old.").AssertAreEqual("OK.");
            rs.reply("What can I do?").AssertAreEqual("Not much of anything.");
            rs.reply("I am 18 years old.").AssertAreEqual("OK.");
            rs.reply("What can I do?").AssertAreEqual("Vote.");
            rs.reply("I am 20 years old.").AssertAreEqual("OK.");
            rs.reply("What can I do?").AssertAreEqual("Vote.");
            rs.reply("I am 22 years old.").AssertAreEqual("OK.");
            rs.reply("What can I do?").AssertAreEqual("Drink.");
            rs.reply("I am 24 years old.").AssertAreEqual("OK.");
            rs.reply("What can I do?").AssertAreEqual("Drink.");
            rs.reply("I am 25 years old.").AssertAreEqual("OK.");
            rs.reply("What can I do?").AssertAreEqual("Rent a car for cheap.");
            rs.reply("I am 27 years old.").AssertAreEqual("OK.");
            rs.reply("What can I do?").AssertAreEqual("Anything you want.");
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

        [TestMethod]
        public void RSTS_Replies__set_uservars()
        {

            var rs = TestHelper.getEmptyStreamed(new[] { "+ what is my name",
                                                         "- Your name is <get name>.",

                                                         "+ how old am i",
                                                         "- You are <get age>."});

            rs.setUservar("name", "Aiden");
            rs.setUservar("age", "5");

            rs.reply("What is my name?").AssertAreEqual("Your name is Aiden.");
            rs.reply("How old am I?").AssertAreEqual("You are 5.");
        }

        [TestMethod]
        public void RSTS_Replies__questionmark()
        {

            var rs = TestHelper.getEmptyStreamed(new[] { "+ google *",
                                                         "- <a href=\"https://www.google.com/search?q=<star>\">Results are here</a>"});


            rs.reply("google coffeescript").AssertAreEqual("<a href=\"https://www.google.com/search?q=coffeescript\">Results are here</a>");
        }

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
        public void RSTS_Replies__previous_2()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "+ hello",
                                                         "- Can you guess my name?",

                                                         "+ *",
                                                         "% can you guess my name",
                                                         "- My name is Jo.",

                                                         "+ [*] jo [*]",
                                                         "% can you guess my name",
                                                         "- Lucky guess ;-)" });

            rs.reply("hello").AssertAreEqual("Can you guess my name?");
            rs.reply("Jo").AssertAreEqual("Lucky guess ;-)");
        }


        [TestMethod]
        public void RSTS_Replies__previous_3()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "+ hello",
                                                         "- Can you guess my name?",

                                                         "+ [*] jo [*]",
                                                         "% can you guess my name",
                                                         "- Lucky guess ;-)",

                                                         "+ *",
                                                         "% can you guess my name",
                                                         "- My name is Jo."});

            rs.reply("hello").AssertAreEqual("Can you guess my name?");
            rs.reply("Jo").AssertAreEqual("Lucky guess ;-)");
        }


    }
}