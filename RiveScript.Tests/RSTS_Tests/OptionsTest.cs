using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests.RSTS
{
    [TestClass]
    public class OptionsTest
    {
        [TestMethod]
        public void RSTS_Options__concat()
        {
            //# The concat option is file scoped and doesn't persist across streams.
            var rs = new RiveScript(Config.Debug);

            rs.stream(new[] { "// Default concat mode = none",
                              "+ test concat default",
                              "- Hello",
                              "^ world!",

                              "! local concat = space",
                              "+ test concat space",
                              "- Hello",
                              "^ world!",

                              "! local concat = none",
                              "+ test concat none",
                              "- Hello",
                              "^ world!",

                              "! local concat = newline",
                              "+ test concat newline",
                              "- Hello",
                              "^ world!",

                              "// invalid concat setting is equivalent to 'none'",
                              "! local concat = foobar",
                              "+ test concat foobar",
                              "- Hello",
                              "^ world!",

                              "// the option is file scoped so it can be left at",
                              "// any setting and won't affect subsequent parses",
                              "! local concat = newline"});


            rs.stream(new[] {  "// concat mode should be restored to the default in a",
                               "// separate file/stream parse",
                               "+ test concat second file",
                               "- Hello",
                               "^ world!"});


            rs.sortReplies();

            rs.reply("test concat default").AssertAreEqual("Helloworld!");
            rs.reply("test concat space").AssertAreEqual("Hello world!");
            rs.reply("test concat none").AssertAreEqual("Helloworld!");
            rs.reply("test concat newline").AssertAreEqual("Hello\nworld!");
            rs.reply("test concat foobar").AssertAreEqual("Helloworld!");
            rs.reply("test concat second file").AssertAreEqual("Helloworld!");
        }



        [TestMethod]
        public void RSTS_Options__concat_newline_with_conditionals()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "! local concat = newline",
                                                         "+ test *",
                                                         "* <star1> == a => First A line",
                                                         "^ Second A line",
                                                         "^ Third A line",
                                                         "- First B line",
                                                         "^ Second B line",
                                                         "^ Third B line" });


            rs.reply("test A").AssertAreEqual("First A line\nSecond A line\nThird A line");
            rs.reply("test B").AssertAreEqual("First B line\nSecond B line\nThird B line");
        }


        [TestMethod]
        public void RSTS_Options__concat_space_with_conditionals()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "! local concat = space",
                                                         "+ test *",
                                                         "* <star1> == a => First A line",
                                                         "^ Second A line",
                                                         "^ Third A line",
                                                         "- First B line",
                                                         "^ Second B line",
                                                         "^ Third B line" });

            rs.reply("test A").AssertAreEqual("First A line Second A line Third A line");
            rs.reply("test B").AssertAreEqual("First B line Second B line Third B line");
        }


        [TestMethod]
        public void RSTS_Options__concat_none_with_conditionals()
        {
            var rs = TestHelper.getEmptyStreamed(new[] { "+ test *",
                                                         "* <star1> == a => First A line",
                                                         "^ Second A line",
                                                         "^ Third A line",
                                                         "- First B line",
                                                         "^ Second B line",
                                                         "^ Third B line" });

            rs.reply("test A").AssertAreEqual("First A lineSecond A lineThird A line");
            rs.reply("test B").AssertAreEqual("First B lineSecond B lineThird B line");
        }

    }
}