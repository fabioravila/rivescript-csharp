using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    [DeploymentItem("TestData/issue-4-rot13/begin.rive", "issue-4-rot13")]
    [DeploymentItem("TestData/issue-4-rot13/main.rive", "issue-4-rot13")]
    public class Issue4Test
    {
        private const string USER = "default";

        [TestMethod]
        public void Substitutio_Rot13_Problem_Load_With_Duscott_Files()
        {
            var rs = new RiveScript(true);
            var loaded = false;

            loaded = rs.loadFile("issue-4-rot13/begin.rive");
            Assert.IsTrue(loaded);

            loaded = rs.loadFile("issue-4-rot13/main.rive");
            Assert.IsTrue(loaded);

            rs.setDebug(true);
            rs.sortReplies();


            var reply = rs.reply(USER, "he's an idiot");


            Assert.IsTrue(rs.IsErrReply(reply));
        }
    }
}
