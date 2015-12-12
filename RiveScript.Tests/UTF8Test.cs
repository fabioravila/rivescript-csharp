using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class UTF8Test
    {
        [TestMethod]
        public void Simple_Reply()
        {
            var rs = new RiveScript(utf8: true, debug: true);

            rs.streamForTest(new[] { "+ olá bot",
                                     "- Olá humano!" });

            var reply = rs.reply("default", "olá bot");

            Assert.AreEqual(reply, "Olá humano!");
        }
    }
}
