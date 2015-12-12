using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.Tests
{
    [TestClass]
    public class OptionalsWithAlternationsTest
    {
        [TestMethod]
        public void Optional_Start_End_Full_Middle_Alternation()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ [*] (when were you born|what is your birthday|what is your bday) [*]",
                              "",
                              "- reply",
                              ""
            });

            rs.sortReplies();

            rs.reply("when were you born").AssertAreEqual("reply");
            rs.reply("tell when were you born").AssertAreEqual("reply");
            rs.reply("tell when were you born please").AssertAreEqual("reply");
            rs.reply("when were you born please").AssertAreEqual("reply");

            rs.reply("what is your birthday").AssertAreEqual("reply");
            rs.reply("tell what is your birthday").AssertAreEqual("reply");
            rs.reply("tell what is your birthday please").AssertAreEqual("reply");
            rs.reply("what is your birthday please").AssertAreEqual("reply");

            rs.reply("what is your bday").AssertAreEqual("reply");
            rs.reply("tell what is your bday").AssertAreEqual("reply");
            rs.reply("tell what is your bday please").AssertAreEqual("reply");
            rs.reply("what is your bday please").AssertAreEqual("reply");
        }


        [TestMethod]
        public void Optional_Start_End_Partial_Middle_Alternation()
        {
            var rs = new RiveScript(true);

            rs.stream(new[] { "+ [*] when is your (birthday|bday) [*]",
                              "",
                              "- reply",
                              ""
            });

            rs.sortReplies();

            rs.reply("when is your birthday").AssertAreEqual("reply");
            rs.reply("tell when is your birthday").AssertAreEqual("reply");
            rs.reply("when is your birthday please").AssertAreEqual("reply");
            rs.reply("tell when is your birthday please").AssertAreEqual("reply");

            rs.reply("when is your bday").AssertAreEqual("reply");
            rs.reply("tell when is your bday").AssertAreEqual("reply");
            rs.reply("when is your bday please").AssertAreEqual("reply");
            rs.reply("tell when is your bday please").AssertAreEqual("reply");
        }
    }
}
