using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class ConditionalTest
    {
        [TestMethod]
        public void Start_After_Condition()
        {


            var rs = TestHelper.getStreamed(new[] { "+ hi1 *",
                                                    "- !<star>!",

                                                    "+ hi2 *",
                                                    "* <get some> != undefined => not possible",
                                                    "- !<star>!"});

            rs.reply("hi1 test")
              .AssertAreEqual("!test!");


            rs.reply("hi2 test")
              .AssertAreEqual("!test!");

        }
    }
}
