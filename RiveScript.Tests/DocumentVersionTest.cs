using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    public class DocumentVersionTest
    {
        [TestMethod]
        public void Load_Older_Version_File()
        {
            var rs = new RiveScript(Config.Debug);

            var result = rs.stream("! version = 1.8");
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void Load_Same_Version_File()
        {
            var rs = new RiveScript(Config.Debug);

            var result = rs.stream("! version = 2.0");
            Assert.IsTrue(result);
        }


        [TestMethod]
        public void Load_Newer_Version_File()
        {
            var rs = new RiveScript(Config.Debug);

            var result = rs.stream("! version = 2.1");
            Assert.IsFalse(result);
        }

    }
}
