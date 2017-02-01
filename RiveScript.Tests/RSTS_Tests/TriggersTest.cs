using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class TriggersTest
    {
        [TestMethod]
        public void RSTS_Triggers__atomic()
        {

            var rs = TestHelper.getEmptyStreamed(new[] { "+ hello bot",
                                                         "- Hello human.",

                                                         "+ what are you",
                                                         "- I am a RiveScript bot." });

            rs.reply("hello bot").AssertAreEqual("Hello human.");
            rs.reply("What are you?").AssertAreEqual("I am a RiveScript bot.");
        }

        [TestMethod]
        public void RSTS_Triggers__wildcards()
        {

            var rs = TestHelper.getEmptyStreamed(new[] { "+ my name is *",
                                                         "- Nice to meet you, <star>.",

                                                         "+ * told me to say *",
                                                         "- Why did <star1> tell you to say <star2>?",

                                                         "+ i am # years old",
                                                         "- A lot of people are <star>.",

                                                         "+ i am _ years old",
                                                         "- Say that with numbers.",

                                                         "+ i am * years old",
                                                         "- Say that with fewer words." });


            rs.reply("My name is Bob").AssertAreEqual("Nice to meet you, bob.");
            rs.reply("Bob told me to say hi").AssertAreEqual("Why did bob tell you to say hi?");
            rs.reply("I am 5 years old").AssertAreEqual("A lot of people are 5.");
            rs.reply("i am five years old").AssertAreEqual("Say that with numbers.");
            rs.reply("i am twenty five years old").AssertAreEqual("Say that with fewer words.");
        }


        [TestMethod]
        public void RSTS_Triggers__alternatives_and_optionals()
        {

            var rs = TestHelper.getEmptyStreamed(new[] { "+ what (are|is) you",
                                                         "- I am a robot.",

                                                         "+ what is your (home|office|cell) [phone] number",
                                                         "- It is 555-1234.",

                                                         "+ [please|can you] ask me a question",
                                                         "- Why is the sky blue?",

                                                         "+ (aa|bb|cc) [bogus]",
                                                         "- Matched.",

                                                         "+ (yo|hi) [computer|bot] *",
                                                         "- Matched." });

            rs.reply("What are you?").AssertAreEqual("I am a robot.");
            rs.reply("What is you?").AssertAreEqual("I am a robot.");

            rs.reply("What is your home phone number?").AssertAreEqual("It is 555-1234.");
            rs.reply("What is your home number?").AssertAreEqual("It is 555-1234.");
            rs.reply("What is your cell phone number?").AssertAreEqual("It is 555-1234.");
            rs.reply("What is your office number?").AssertAreEqual("It is 555-1234.");

            rs.reply("Can you ask me a question?").AssertAreEqual("Why is the sky blue?");
            rs.reply("Please ask me a question").AssertAreEqual("Why is the sky blue?");
            rs.reply("ask me a question.").AssertAreEqual("Why is the sky blue?");

            //Test that optionals won't match when touching other parts of the message.
            rs.reply("aa").AssertAreEqual("Matched.");
            rs.reply("bb").AssertAreEqual("Matched.");
            rs.reply("aa bogus").AssertAreEqual("Matched.");
            rs.reply("aabogus").AssertAreEqual("ERR: No Reply Matched");
            rs.reply("bogus").AssertAreEqual("ERR: No Reply Matched");


            rs.reply("hi Aiden").AssertAreEqual("Matched.");
            rs.reply("hi bot how are you?").AssertAreEqual("Matched.");
            rs.reply("yo computer what time is it?").AssertAreEqual("Matched.");
            rs.reply("yoghurt is yummy").AssertAreEqual(rs.errors.replyNotMatched);
            rs.reply("hide and seek is fun").AssertAreEqual(rs.errors.replyNotMatched);
            rs.reply("hip hip hurrah").AssertAreEqual(rs.errors.replyNotMatched);

        }



        [TestMethod]
        public void RSTS_Triggers__trigger_arrays()
        {

            var rs = TestHelper.getEmptyStreamed(new[] { "! array colors = red blue green yellow white",
                                                         "  ^ dark blue|light blue",
                                                         "+ what color is my (@colors) *",
                                                         "- Your <star2> is <star1>.",

                                                         "+ what color was * (@colors) *",
                                                         "- It was <star2>.",

                                                         "+ i have a @colors *",
                                                         "- Tell me more about your <star>." });

            rs.reply("What color is my red shirt?").AssertAreEqual("Your shirt is red.");
            rs.reply("What color is my blue car?").AssertAreEqual("Your car is blue.");
            rs.reply("What color is my pink house?").AssertAreEqual(rs.errors.replyNotMatched);
            rs.reply("What color is my dark blue jacket?").AssertAreEqual("Your jacket is dark blue.");
            rs.reply("What color was Napoleon's white horse?").AssertAreEqual("It was white.");
            rs.reply("What color was my red shirt?").AssertAreEqual("It was red.");
            rs.reply("I have a blue car.").AssertAreEqual("Tell me more about your car.");
            rs.reply("I have a cyan car.").AssertAreEqual(rs.errors.replyNotMatched);
        }


        [TestMethod]
        public void RSTS_Triggers__weighted_triggers()
        {
            var rs = TestHelper.getEmptyStreamed(new[] {
                                                            "+ * or something{weight=10}",
                                                            "- Or something. <@>",

                                                            "+ can you run a google search for *",
                                                            "- Sure!",

                                                            "+ hello *{weight=20}",
                                                            "- Hi there!",

                                                            "// Test that spaces before or after the {weight} tag are gobbled up along",
                                                            "// with the {weight} tag itself.",

                                                            "+ something{weight=100}",
                                                            "- Weighted something",

                                                            "+ something",
                                                            "- Unweighted something",

                                                            "+ nothing {weight=100}",
                                                            "- Weighted nothing",

                                                            "+ nothing",
                                                            "- Unweighted nothing",

                                                            "+ {weight=100}everything",
                                                            "- Weighted everything",

                                                            "+ everything",
                                                            "- Unweighted everything",

                                                            "+ {weight=100}   blank",
                                                            "- Weighted blank",

                                                            "+ blank",
                                                            "- Unweighted blank" });

            rs.reply("Hello robot.").AssertAreEqual("Hi there!");
            rs.reply("Hello or something.").AssertAreEqual("Hi there!");
            rs.reply("Can you run a Google search for Node?").AssertAreEqual("Sure!");
            rs.reply("Can you run a Google search for Node or something?").AssertAreEqual("Or something. Sure!");
            rs.reply("something").AssertAreEqual("Weighted something");
            rs.reply("nothing").AssertAreEqual("Weighted nothing");
            rs.reply("everything").AssertAreEqual("Weighted everything");
            rs.reply("blank").AssertAreEqual("Weighted blank");
        }
    }
}