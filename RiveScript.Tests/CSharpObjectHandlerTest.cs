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
            var rs = new RiveScript();
            var oh = new CSharp();

            oh.onLoad("test", new string[] { "return \"Hello world\"; " });

            var result = oh.onCall("test", rs, new string[] { "" });

            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void Hello_World_Reply_RS_Instance()
        {
            var rs = new RiveScript();
            var oh = new CSharp();

            oh.onLoad("test", new string[] { "return rs.GetHashCode().ToString(); " });

            var result = oh.onCall("test", rs, new string[] { "" });

            Assert.AreEqual(rs.GetHashCode().ToString(), result);
        }

        [TestMethod]
        public void Hello_World_Reply_Concatenet_Args_Id()
        {
            var rs = new RiveScript();
            var oh = new CSharp();

            oh.onLoad("test", new string[] { "return String.Join(\",\", args); " });

            var result = oh.onCall("test", rs, new string[] { "1", "2", "3" });

            Assert.AreEqual("1,2,3", result);
        }


        [TestMethod]
        public void Hello_World_Reply_CurrentUser_Not_Initialized()
        {
            var rs = new RiveScript();
            var oh = new CSharp();

            oh.onLoad("test", new string[] { "return rs.currentUser();" });

            var result = oh.onCall("test", rs, new string[] { "" });

            Assert.AreEqual(Constants.Undefined, result);
        }
    }
}
