using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests
{
    [TestClass]
    public class RiveScript2TestSuite
    {
        /*
        * [Atomic Reply]
        */
        [TestMethod]
        public void AtomicReply_Basic()
        {
            var rs = TestHelper.getStreamed(new[] { "+ hello bot",
                                                    "- Hello human." });
            var result = rs.reply("hello bot");
            Assert.AreEqual("Hello human.", result);
        }

        [TestMethod]
        public void AtomicReply_With_Bot_Var_Reply()
        {
            var rs = TestHelper.getStreamed(new[] { "+ what is your name",
                                                    "- You can call me <bot name>." });
            var result = rs.reply(" what is your name");
            Assert.AreEqual("You can call me RiveScript Test Bot.", result);
        }

        /*
        * [WildCards]
        */
        [TestMethod]
        public void WildCard_Basic()
        {
            var rs = TestHelper.getStreamed(new[] { "+ my favorite thing in the world is *",
                                                    "- Why do you like <star> so much?" });
            var result = rs.reply("my favorite thing in the world is programming");
            Assert.AreEqual("Why do you like programming so much?", result);
        }

        [TestMethod]
        public void WildCard_Multiple()
        {
            var rs = TestHelper.getStreamed(new[] { "+ * told me to say *",
                                                    "- Why would <star1> have told you to say <star2>?" });
            var result = rs.reply("John told me to say hello");
            Assert.AreEqual("Why would john have told you to say hello?", result);
        }

        [TestMethod]
        public void WildCard_Basic_2()
        {
            var rs = TestHelper.getStreamed(new[] { "+ i think *",
                                                    "- Do you think <star> a lot?" });
            var result = rs.reply("I think the sky is orange.");
            Assert.AreEqual("Do you think the sky is orange a lot?", result);
        }

        [TestMethod]
        public void WildCard_Priority()
        {
            /*When multiple triggers exist that are identical except for
            * their wildcard character, the order of priorities are that
            * _ is always first, # is second, and * last. So in this code
            * and the following one, the "i am # years old" should match
            * if the wildcard is a number and the "i am * years old" should
            *  only match otherwise.
            */

            var rs = TestHelper.getStreamed(new[] { "+ i am * years old",
                                                    "- Tell me that as a number instead of spelled out like \"<star>\".",
                                                    "+ i am # years old",
                                                    "- <set age=<star>>I will remember that you are <star> years old.",
                                                    "+ what is my age",
                                                    "- your age is <get age>",
                                                    ""});

            var r1 = rs.reply("I am twenty years old");
            Assert.AreEqual(r1, "Tell me that as a number instead of spelled out like \"twenty\".");

            //This reply should also set the var "age" to 20 for this user.
            var r2 = rs.reply("I am 20 years old");
            Assert.AreEqual(r2, "I will remember that you are 20 years old.");

            var r3 = rs.reply("What is my age");
            Assert.AreEqual(r3, "your age is 20");
        }


        /*
        * [Alternations]
        * Skip Alternations Test, I just have
        */

        /*
        * [Optionals]
        * Skip Optionals Test, I just have
        */

        /*
        * [Arrays]
        */
        [TestMethod]
        public void Array_Color()
        {
            var rs = TestHelper.getStreamed(new[] { "+ what color is my (@colors) shirt",
                                                    "- Your shirt is <star>, silly." });
            var r1 = rs.reply("What color is my blue shirt?");
            var r2 = rs.reply("What color is my light red shirt?");
            var r3 = rs.reply("What color is my black shirt?");

            Assert.AreEqual("Your shirt is blue, silly.", r1);
            Assert.AreEqual("Your shirt is light red, silly.", r2);
            Assert.AreEqual("Your shirt is black, silly.", r3);
        }

        [TestMethod]
        public void Array_Color_With_Wild_Card()
        {
            var rs = TestHelper.getStreamed(new[] { "+ what color was * (@colors) *",
                                                    "- <formal> <star3> was <star2>." });
            var r1 = rs.reply("What color was George Washington's white horse?");
            Assert.AreEqual("George Washingtons horse was white.", r1);
        }

        [TestMethod]
        public void Array_Color_WithWildCard2()
        {
            var rs = TestHelper.getStreamed(new[] { "+ i have a @colors *",
                                                    "- Why did you choose that color for a <star>?" });
            var r1 = rs.reply("I have a yellow sports car");
            Assert.AreEqual("Why did you choose that color for a sports car?", r1);
        }

        /*
        * [Priority Triggers]
        */
        [TestMethod]
        public void Priority_Triggers()
        {
            /* This would normally match the color trigger, but this one has
             * a high priority and matches first, even though the trigger
             * above has more words and is a more specific match.
             */

            var rs = TestHelper.getStreamed(new[] { "+ i have a @colors *",
                                                    "- Why did you choose that color for a<star> ? ",
                                                    "+ {weight=100}* davenport",
                                                    "- That's a word that's not used much anymore." });
            var result = rs.reply("I have a black davenport");
            Assert.AreEqual("That's a word that's not used much anymore.", result);
        }


        /*
        * [Basic Reply Testing ]
        */

        [TestMethod]
        public void Atomic_Response()
        {
            var rs = TestHelper.getStreamed(new[] { "+ how are you",
                                                    "- I'm great." });
            var result = rs.reply("how are you");
            Assert.AreEqual("I'm great.", result);
        }

        [TestMethod]
        public void Rendom_Response()
        {
            var rs = TestHelper.getStreamed(new[] { "+ (hello|hi|hey)",
                                                    "- Hey there!",
                                                    "- Hello!",
                                                    "- Hi!"});

            var r1 = rs.reply("hello");
            var r2 = rs.reply("hi");
            var r3 = rs.reply("hey");

            var expected = new[] { "Hey there!", "Hello!", "Hi!" };

            Assert.IsTrue(expected.Contains(r1));
            Assert.IsTrue(expected.Contains(r2));
            Assert.IsTrue(expected.Contains(r3));
        }

        [TestMethod]
        public void Random_Response_2()
        {
            var rs = TestHelper.getStreamed(new[] { "+ my name is *",
                                                    "- <set name=<formal>>Nice to meet you, <formal>.",
                                                    "- <set name=<formal>>Hi, <formal>, my name is <bot name>.",
                                                    "- <set name=<formal>><formal>, nice to meet you.",
                                                    "+ what is my name",
                                                    "- your name is <get name>"
            });

            //This would also set the var name=Casey for the user.
            var r1 = rs.reply("my name is Casey");
            var r2 = rs.reply("what is my name");

            var expected = new[] { "Nice to meet you, Casey.",
                                   "Hi, Casey, my name is RiveScript Test Bot.",
                                   "Casey, nice to meet you." };

            Assert.IsTrue(expected.Contains(r1));
            Assert.AreEqual("your name is Casey", r2);
        }

        [TestMethod]
        public void Weighted_Random_Response()
        {
            //TODO: Make A foreach to execute several times ans extract statistics

            var rs = TestHelper.getStreamed(new[] { "+ tell me a secret",
                                                    "- I won't tell you a secret.{weight=20}",
                                                    "- You can't handle a secret.{weight=20}",
                                                    "- Okay, here's a secret... nope, just kidding.{weight=5}",
                                                    "- Actually, I just don't have any secrets.",
                                                    ""
            });

            //This would also set the var name=Casey for the user.
            var r1 = rs.reply("Tell me a secret");

            var expected = new[] { "I won't tell you a secret.",
                                   "You can't handle a secret.",
                                   "Okay, here's a secret... nope, just kidding.",
                                   "Actually, I just don't have any secrets." };


            Assert.IsTrue(expected.Contains(r1));
        }


        /*
        * [Command Testing]
        */
        [TestMethod]
        public void Previous()
        {
            var rs = TestHelper.getStreamed(new[] { "+ knock knock",
                                                    "- Who's there?",
                                                    "",
                                                    "+ *",
                                                    "% who is there",
                                                    "- <set joke=<sentence>><sentence> who?",
                                                    "",
                                                    "+ <get joke> *",
                                                    "- Haha! \"{sentence}<get joke> <star>{/sentence}\"! :D",
                                                    ""});


            rs.reply("Knock, knock.")
              .AssertAreEqual("Who's there?");
            rs.reply("Banana.")
              .AssertAreEqual("Banana who?");
            rs.reply("Knock, knock.")
              .AssertAreEqual("Who's there?");
            rs.reply("Banana.")
              .AssertAreEqual("Banana who?");
            rs.reply("Knock, knock.")
              .AssertAreEqual("Who's there?");
            rs.reply("Orange.")
              .AssertAreEqual("Orange who?");
            rs.reply("Orange you glad i didn't say banana?")
              .AssertAreEqual("Haha! \"Orange you glad i did not say banana\"! :D");
        }


        [TestMethod]
        public void Continue()
        {
            var rs = TestHelper.getStreamed(new[] { "+ tell me a poem",
                                                    "- Little Miss Muffit sat on her tuffet\n",
                                                    "^ in a nonchalant sort of way.\n",
                                                    "^ With her forcefield around her,\n",
                                                    "^ the Spider, the bounder,\n",
                                                    "^ Is not in the picture today.",
                                                    "",
                                                    "",
                                                    "",                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
                                                    ""});


            rs.reply("Tell me a poem").AssertAreEqual("Little Miss Muffit sat on her tuffet\nin a nonchalant sort of way.\nWith her forcefield around her,\nthe Spider, the bounder,\nIs not in the picture today.");






        }

    }
}

