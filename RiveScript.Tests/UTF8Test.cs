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


        [TestMethod]
        public void Complex_Random_With_Conditionals_And_Arrays_Triggers_UTF8()
        {
            var rs = new RiveScript(utf8: true);

            rs.streamForTest(new[] { "! array oi = olá oi",
                                     "^ boa noite|bom dia",
                                     "",
                                     "+ meu nome é *",
                                     "- <set name=<formal>>Prazer em conhecer, <get name>",
                                     "",
                                     "+ @oi",
                                     "* <get name> != undefined => {random}",
                                     "^ Olá, <get name>.|",
                                     "^ Bom dia, <get name>.{/random}",
                                     "- Olá!",
                                     "- Como vai?",
                                    });

            //Without name

            rs.reply("olá")
              .AssertContains("Olá!", "Como vai?");

            //Set name
            rs.reply("meu nome é Fábio")
              .AssertAreEqual("Prazer em conhecer, Fábio");


            rs.reply("bom dia")
               .AssertContains("Olá, Fábio.", "Bom dia, Fábio.");
        }

    }
}
