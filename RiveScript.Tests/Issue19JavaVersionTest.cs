using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RiveScript.Tests
{
    [TestClass]
    [DeploymentItem("TestData/issue-19-start-java-version/begin.rive", "issue-19-start-java-version")]
    [DeploymentItem("TestData/issue-19-start-java-version/main.rive", "issue-19-start-java-version")]
    public class Issue19JavaVersionTest
    {
        private const string USER = "default";

        [TestMethod]
        public void Start_Has_Work_After_Condition()
        {
            var rs = new RiveScript(Config.DebugUTF8);
            var loaded = false;

            loaded = rs.loadFile("issue-19-start-java-version/begin.rive");
            Assert.IsTrue(loaded);

            loaded = rs.loadFile("issue-19-start-java-version/main.rive");
            Assert.IsTrue(loaded);

            rs.setDebug(true);
            rs.sortReplies();


            var reply1 = rs.reply(USER, "hi1 test");
            Assert.AreEqual("!test!", reply1);

            var reply2 = rs.reply(USER, "hi2 test");
            Assert.AreEqual("!test!", reply2);
        }
    }
}
