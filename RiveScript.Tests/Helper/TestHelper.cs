
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace RiveScript.Tests
{
    public static class TestHelper
    {
        public static string ErrorNoReplyMatched = "ERR: No Reply Matched";

        public static RiveScript getStreamed(string[] code)
        {
            var rs = new RiveScript(false);

            //Stram bais variables for teste suite
            rs.stream(new[] { "// Bot Variables",
                              "! var name     = RiveScript Test Bot",
                              "! var age      = 9000",
                              "! var gender   = androgynous",
                              "! var location = Cyberspace",
                              "! var phone    = 555-1234",
                              "! var email    = test@mydomain.com",
                              "// Substitutions",
                              "! sub +         = plus",
                              "! sub -         = minus",
                              "! sub /         = divided",
                              "! sub *         = times",
                              "! sub i'm       = i am",
                              "! sub i'd       = i would",
                              "! sub i've      = i have",
                              "! sub i'll      = i will",
                              "! sub don't     = do not",
                              "! sub isn't     = is not",
                              "! sub you'd     = you would",
                              "! sub you're    = you are",
                              "! sub you've    = you have",
                              "! sub you'll    = you will",
                              "! sub he'd      = he would",
                              "! sub he's      = he is",
                              "! sub he'll     = he will",
                              "! sub she'd     = she would",
                              "! sub she's     = she is",
                              "! sub she'll    = she will",
                              "! sub they'd    = they would",
                              "! sub they're   = they are",
                              "! sub they've   = they have",
                              "! sub they'll   = they will",
                              "! sub we'd      = we would",
                              "! sub we're     = we are",
                              "! sub we've     = we have",
                              "! sub we'll     = we will",
                              "! sub whats     = what is",
                              "! sub what's    = what is",
                              "! sub what're   = what are",
                              "! sub what've   = what have",
                              "! sub what'll   = what will",
                              "! sub can't     = can not",
                              "! sub whos      = who is",
                              "! sub who's     = who is",
                              "! sub who'd     = who would",
                              "! sub who'll    = who will",
                              "! sub don't     = do not",
                              "! sub didn't    = did not",
                              "! sub it's      = it is",
                              "! sub could've  = could have",
                              "! sub couldn't  = could not",
                              "! sub should've = should have",
                              "! sub shouldn't = should not",
                              "! sub would've  = would have",
                              "! sub wouldn't  = would not",
                              "! sub when's    = when is",
                              "! sub when're   = when are",
                              "! sub when'd    = when did",
                              "! sub y         = why",
                              "! sub u         = you",
                              "! sub ur        = your",
                              "! sub r         = are",
                              "! sub im        = i am",
                              "! sub wat       = what",
                              "! sub wats      = what is",
                              "! sub ohh       = oh",
                              "! sub becuse    = because",
                              "! sub becasue   = because",
                              "! sub becuase   = because",
                              "! sub practise  = practice",
                              "! sub its a     = it is a",
                              "! sub fav       = favorite",
                              "! sub fave      = favorite",
                              "! sub iam       = i am",
                              "! sub realy     = really",
                              "! sub iamusing  = i am using",
                              "! sub amleaving = am leaving",
                              "! sub yuo       = you",
                              "! sub youre     = you are",
                              "! sub didnt     = did not",
                              "! sub ain't     = is not",
                              "! sub aint      = is not",
                              "! sub wanna     = want to",
                              "! sub brb       = be right back",
                              "! sub bbl       = be back later",
                              "! sub gtg       = got to go",
                              "! sub g2g       = got to go",
                              "",
                              "// Person substitutions",
                              "! person i am    = you are",
                              "! person you are = I am",
                              "! person i'm     = you're",
                              "! person you're  = I'm",
                              "! person my      = your",
                              "! person your    = my",
                              "! person you     = I",
                              "! person i       = you",
                              "",
                              "// Arrays",
                              "! array colors = red green blue cyan yellow magenta white orange brown black",
                              "  ^ gray grey fuchsia maroon burgundy lime navy aqua gold silver copper bronze",
                              "  ^ light red|light green|light blue|light cyan|light yellow|light magenta",
                              "! array be     = is are was were",
                              ""});




            rs.setDebug(true);
            rs.stream(code);
            rs.sortReplies();
            return rs;
        }

        public static void streamForTest(this RiveScript rs, string[] code)
        {
            rs.stream(code);
            rs.sortReplies();
        }

        public static string reply(this RiveScript rs, string message)
        {
            return rs.reply("default", message);
        }


        public static void AssertAreEqual<T>(this T target, T expected)
        {
            Assert.AreEqual(expected, target);
        }

        public static void AssertAreNotEqual<T>(this T target, T expected)
        {
            Assert.AreNotEqual(expected, target);
        }

        public static void AssertContains<T>(this T expected, T[] target)
        {
            Assert.IsTrue(target.Contains(expected));
        }

    }
}


