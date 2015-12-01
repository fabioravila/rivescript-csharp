using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiveScript.lang;

namespace RiveScript.Tests
{
    [TestClass]
    public class CSharpObjectHandlerTest
    {
        [TestMethod]
        public void Hello_World_Simple_Code()
        {
            var or = new CSharp();

            or.onLoad("test", new string[] { "return \"Hello world\"; " });

            var result = or.onCall("test", "default", new string[] { "" });

            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void Hello_World_Reply_User_Id()
        {
            var or = new CSharp();

            or.onLoad("test", new string[] { "return user; " });

            var result = or.onCall("test", "default", new string[] { "" });

            Assert.AreEqual("default", result);
        }

        [TestMethod]
        public void Hello_World_Reply_Concatenet_Args_Id()
        {
            var or = new CSharp();

            or.onLoad("test", new string[] { "return String.Join(\",\", args); " });

            var result = or.onCall("test", "default", new string[] { "1", "2", "3" });

            Assert.AreEqual("1,2,3", result);
        }
    }
}
