using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class UnicodeTest
    {
        [TestMethod]
        public void RSTS_Unicode__unicode()
        {
            var rs = new RiveScript(debug: true, utf8: true);

            rs.stream(new[] { "! sub who's = who is",
                              "+ äh",
                              "- What's the matter?",

                              "+ ブラッキー",
                              "- エーフィ",

                              "// Make sure %Previous continues working in UTF-8 mode.",
                              "+ knock knock",
                              "- Who's there?",

                              "+ *",
                              "% who is there",

                              "- <sentence> who?",
                              "+ *",
                              "% * who",
                              "- Haha! <sentence>!",

                              "// And with UTF-8.",
                              "+ tëll më ä pöëm",
                              "- Thërë öncë wäs ä män nämëd Tïm",

                              "+ more",
                              "% thërë öncë wäs ä män nämëd tïm",
                              "- Whö nëvër qüïtë lëärnëd höw tö swïm",

                              "+ more",
                              "% whö nëvër qüïtë lëärnëd höw tö swïm",
                              "- Hë fëll öff ä döck, änd sänk lïkë ä röck",

                              "+ more",
                              "% hë fëll öff ä döck änd sänk lïkë ä röck",
                              "- Änd thät wäs thë ënd öf hïm."});

            rs.sortReplies();

            rs.reply("äh").AssertAreEqual("What's the matter?");
            rs.reply("ブラッキー").AssertAreEqual("エーフィ");
            rs.reply("knock knock").AssertAreEqual("Who's there?");
            rs.reply("orange").AssertAreEqual("Orange who?");
            rs.reply("banana").AssertAreEqual("Haha! Banana!");
            rs.reply("tëll më ä pöëm").AssertAreEqual("Thërë öncë wäs ä män nämëd Tïm");
            rs.reply("more").AssertAreEqual("Whö nëvër qüïtë lëärnëd höw tö swïm");
            rs.reply("more").AssertAreEqual("Hë fëll öff ä döck, änd sänk lïkë ä röck");
            rs.reply("more").AssertAreEqual("Änd thät wäs thë ënd öf hïm.");
        }


        [TestMethod]
        public void RSTS_Unicode__wildcard()
        {
            var rs = new RiveScript(debug: true, utf8: true);

            rs.stream(new[] {   "+ my name is _",
                                "- Nice to meet you, <star>.",

                                "+ i am # years old",
                                "- A lot of people are <star> years old.",

                                "+ *",
                                "- No match."});

            rs.sortReplies();

            rs.reply("My name is Aiden").AssertAreEqual("Nice to meet you, aiden.");
            rs.reply("My name is Bảo").AssertAreEqual("Nice to meet you, bảo.");
            rs.reply("My name is 5").AssertAreEqual("No match.");
            rs.reply("I am five years old").AssertAreEqual("No match.");
            rs.reply("I am 5 years old").AssertAreEqual("A lot of people are 5 years old.");


        }
    }
}