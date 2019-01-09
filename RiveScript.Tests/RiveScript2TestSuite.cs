using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests
{
    /*
     * Tests based on RiveScript 2 Test Suite -- Designed to demonstrate all the
	 * functionality that RiveScript 2 is supposed to support.
     */
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
            rs.reply("hello bot")
              .AssertAreEqual("Hello human.");
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

            /* When multiple triggers exist that are identical except for
             * their wildcard character, the order of priorities are that
             * _ is always first, # is second, and * last. So in this code
             * and the following one, the "i am # years old" should match
             * if the wildcard is a number and the "i am * years old" should
             * only match otherwise.
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

            // This reply should also set the var "age" to 20 for this user.
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


            rs.reply("What color was George Washington's white horse?")
              .AssertAreEqual("George Washingtons horse was white.");
        }

        [TestMethod]
        public void Array_Color_WithWildCard2()
        {
            var rs = TestHelper.getStreamed(new[] { "+ i have a @colors *",
                                                    "- Why did you choose that color for a <star>?" });
            rs.reply("I have a yellow sports car")
              .AssertAreEqual("Why did you choose that color for a sports car?");
        }

        /*
        * [Priority Triggers]
        */
        [TestMethod]
        public void Priority_Triggers()
        {
            /*
             * This would normally match the trigger above, but this one has
             * a high priority and matches first, even though the trigger
             * above has more words and is a more specific match.
             */

            var rs = TestHelper.getStreamed(new[] { "+ i have a @colors *",
                                                    "- Why did you choose that color for a<star> ? ",
                                                    "+ {weight=100}* davenport",
                                                    "- That's a word that's not used much anymore." });


            rs.reply("I have a black davenport")
              .AssertAreEqual("That's a word that's not used much anymore.");
        }

        /*
        * [Basic Reply Testing ]
        */

        [TestMethod]
        public void Atomic_Response()
        {
            var rs = TestHelper.getStreamed(new[] { "+ how are you",
                                                    "- I'm great." });
            rs.reply("how are you")
              .AssertAreEqual("I'm great.");
        }

        [TestMethod]
        public void Rendom_Response()
        {
            var rs = TestHelper.getStreamed(new[] { "+ (hello|hi|hey)",
                                                    "- Hey there!",
                                                    "- Hello!",
                                                    "- Hi!"});

            var expected = new[] { "Hey there!", "Hello!", "Hi!" };


            rs.reply("hello")
              .AssertContains(expected);

            rs.reply("hi")
              .AssertContains(expected);

            rs.reply("hey")
              .AssertContains(expected);
        }

        [TestMethod]
        public void Random_Response_2()
        {

            // Extra notes:    This would also set the var name = Casey for the user.


            var rs = TestHelper.getStreamed(new[] { "+ my name is *",
                                                    "- <set name=<formal>>Nice to meet you, <formal>.",
                                                    "- <set name=<formal>>Hi, <formal>, my name is <bot name>.",
                                                    "- <set name=<formal>><formal>, nice to meet you.",
                                                    "+ what is my name",
                                                    "- your name is <get name>"
            });

            // This would also set the var name=Casey for the user.
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
            // TODO: Make A foreach to execute several times ans extract statistics
            var rs = TestHelper.getStreamed(new[] { "+ tell me a secret",
                                                    "- I won't tell you a secret.{weight=20}",
                                                    "- You can't handle a secret.{weight=20}",
                                                    "- Okay, here's a secret... nope, just kidding.{weight=5}",
                                                    "- Actually, I just don't have any secrets.",
                                                    ""
            });

            // This would also set the var name=Casey for the user.
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
            // Human says:     Knock, knock.
            // Expected reply: Who's there?
            // Human says:     Banana.
            // Expected reply: Banana who?
            // Human says:     Knock, knock.
            // Expected reply: Who's there?
            // Human says:     Banana.
            // Expected reply: Banana who?
            // Human says:     Knock, knock.
            // Expected reply: Who's there?
            // Human says:     Orange.
            // Expected reply: Orange who?
            // Human says:     Orange you glad I didn't say banana?
            // Expected reply: Haha!"Orange you glad I didn't say banana"! :D


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
            var rs = TestHelper.getEmptyStreamed(new[] { "+ tell me a poem",
                                                    "- Little Miss Muffit sat on her tuffet\n",
                                                    "^ in a nonchalant sort of way.\n",
                                                    "^ With her forcefield around her,\n",
                                                    "^ the Spider, the bounder,\n",
                                                    "^ Is not in the picture today.",
                                                    ""});


            rs.reply("Tell me a poem").AssertAreEqual("Little Miss Muffit sat on her tuffet\n" +
                                                      "in a nonchalant sort of way.\n" +
                                                      "With her forcefield around her,\n" +
                                                      "the Spider, the bounder,\n" +
                                                      "Is not in the picture today.");
        }

        /*
        * [Redirects]
        */
        [TestMethod]
        public void Redirect()
        {
            var rs = TestHelper.getStreamed(new[] { "+ what is your name",
                                                    "- You can call me <bot name>.",
                                                    "",
                                                    "+ who are you",
                                                    "@ what is your name",
                                                    ""});

            rs.reply("Who are you?")
              .AssertAreEqual("You can call me RiveScript Test Bot.");
        }

        [TestMethod]
        public void Redirect_DeepRecursion()
        {
            var rs = TestHelper.getStreamed(new[] { "+ test recursion",
                                                    "@ test more recursion",
                                                    "",
                                                    "+ test more recursion",
                                                    "@ test recursion",
                                                    ""});

            rs.reply("Test recursion")
              .AssertAreEqual(rs.errors.deepRecursion);
        }

        /*
         * Conditionals
        */
        [TestMethod]
        public void Conditionals_Age()
        {
            var rs = TestHelper.getStreamed(new[] { "+ what am i old enough to do",
                                                    "* <get age> == undefined => You never told me how old you are.",
                                                    "* <get age> >= 21        => You're over 21 so you can drink.",
                                                    "* <get age> >= 18        => You're over 18 so you can gamble.",
                                                    "* <get age> <  18        => You're too young to do much of anything.",
                                                    "- This reply shouldn't happen.",
                                                    "",
                                                    "+ i am # years old",
                                                    "- <set age=<star>>ok",
                                                    ""});

            rs.reply("what am i old enough to do")
              .AssertAreEqual("You never told me how old you are.");

            rs.reply("i am 21 years old").AssertAreEqual("ok");

            rs.reply("what am i old enough to do")
              .AssertAreEqual("You're over 21 so you can drink.");

            rs.reply("i am 22 years old").AssertAreEqual("ok");

            rs.reply("what am i old enough to do")
              .AssertAreEqual("You're over 21 so you can drink.");

            rs.reply("i am 18 years old").AssertAreEqual("ok");

            rs.reply("what am i old enough to do")
              .AssertAreEqual("You're over 18 so you can gamble.");

            rs.reply("i am 19 years old").AssertAreEqual("ok");

            rs.reply("what am i old enough to do")
              .AssertAreEqual("You're over 18 so you can gamble.");

            rs.reply("i am 17 years old").AssertAreEqual("ok");

            rs.reply("what am i old enough to do")
              .AssertAreEqual("You're too young to do much of anything.");
        }

        [TestMethod]
        public void Conditionals_Age_2()
        {
            var rs = TestHelper.getStreamed(new[] { "+ am i 18 years old",
                                                    "* <get age> == undefined => I don't know how old you are.",
                                                    "* <get age> != 18        => You're not 18, no.",
                                                    "- Yes, you are.",
                                                    "",
                                                    "+ i am # years old",
                                                    "- <set age=<star>>ok",
                                                    ""});


            rs.reply("Am I 18 years old?")
              .AssertAreEqual("I don't know how old you are.");

            rs.reply("i am 17 years old").AssertAreEqual("ok");

            rs.reply("Am I 18 years old?")
              .AssertAreEqual("You're not 18, no.");

            rs.reply("i am 19 years old").AssertAreEqual("ok");

            rs.reply("Am I 18 years old?")
              .AssertAreEqual("You're not 18, no.");

            rs.reply("i am 18 years old").AssertAreEqual("ok");

            rs.reply("Am I 18 years old?")
              .AssertAreEqual("Yes, you are.");
        }

        [TestMethod]
        public void Conditionals_Number_Game_Operators()
        {
            var rs = TestHelper.getStreamed(new[] { "+ count",
                                                    "* <get count> == undefined => <set count=1>Let's start with 1.",
                                                    "* <get count> == 0         => <set count=1>Let's start again with 1.",
                                                    "* <get count> == 1         => <add count=1>I've added 1 to the count.",
                                                    "* <get count> == 2         => <add count=5>I've added 5 now.",
                                                    "* <get count> == 3         => <add count=3>Now I've added 3.",
                                                    "* <get count> == 4         => <sub count=1>Subtracted 1.",
                                                    "* <get count> == 5         => <mult count=2>Now I've doubled that.",
                                                    "* <get count> == 6         => <add count=3>Added 3 again.",
                                                    "* <get count> == 7         => <sub count=2>Subtracted 2.",
                                                    "* <get count> == 8         => <div count=2>Divided that by 2.",
                                                    "* <get count> == 9         => <set count=0>We're done. Do you know what number I",
                                                    "  ^ \\sstopped at?",
                                                    "* <get count> == 10        => <sub count=2>Subtracted 2 from that now.",
                                                    "",
                                                    "+ (9|nine)",
                                                    "% * do you know what number i stopped at",
                                                    "- You're right, I stopped at the number 9. :)",
                                                    ""});

            rs.reply("Count.").AssertAreEqual("Let's start with 1.");
            rs.reply("Count.").AssertAreEqual("I've added 1 to the count.");
            rs.reply("Count.").AssertAreEqual("I've added 5 now.");
            rs.reply("Count.").AssertAreEqual("Subtracted 2.");
            rs.reply("Count.").AssertAreEqual("Now I've doubled that.");
            rs.reply("Count.").AssertAreEqual("Subtracted 2 from that now.");
            rs.reply("Count.").AssertAreEqual("Divided that by 2.");
            rs.reply("Count.").AssertAreEqual("Subtracted 1.");
            rs.reply("Count.").AssertAreEqual("Now I've added 3.");
            rs.reply("Count.").AssertAreEqual("Added 3 again.");
            rs.reply("Count.").AssertAreEqual("We're done. Do you know what number I stopped at?");
            rs.reply("9").AssertAreEqual("You're right, I stopped at the number 9. :)");
        }


        /*
         * [Topic Testing]
        */
        [TestMethod]
        public void Temporarily_Ignore_Abusive_Users()
        {
            var rs = TestHelper.getStreamed(new[] { "+ insert swear word here",
                                                    "- Omg you're mean! Apologize.{topic=apology}",
                                                    "",
                                                    "> topic apology",
                                                    "   + *",
                                                    "	- Not until you apologize.",
                                                    "	- Say you're sorry.",
                                                    "	- Apologize for being so mean.",
                                                    "",
                                                    "	+ [*] (sorry|apologize) [*]",
                                                    "	- Okay, I'll forgive you.{topic=random}",
                                                    "< topic",
                                                    "",
                                                    "+ hey",
                                                    "- hello",
                                                    ""});

            rs.reply("hey")
                 .AssertAreEqual("hello");

            rs.reply("insert swear word here")
              .AssertAreEqual("Omg you're mean! Apologize.");

            var expected = new[] { "Not until you apologize.", "Say you're sorry.", "Apologize for being so mean." };
            for (int i = 0; i < 5; i++)
            {
                rs.reply("hey")
                    .AssertContains(expected);
            }

            rs.reply("sorry")
             .AssertAreEqual("Okay, I'll forgive you.");

            rs.reply("hey")
               .AssertAreEqual("hello");
        }



        //TODO: Topic Inheritence (simple roleplaying game)
    }
}

