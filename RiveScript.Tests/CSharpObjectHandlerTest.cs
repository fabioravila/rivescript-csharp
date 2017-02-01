using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RiveScript.lang;
using RiveScript.Tests.Helper;

namespace RiveScript.Tests
{
    [TestClass]
    public class CSharpObjectHandlerTest
    {
        [TestMethod]
        public void Hello_World_Simple_Code_AndChsrpHandlerIsDefault()
        {
            var rs = new RiveScript();
            var oh = new CSharp();

            oh.onLoad("test", new string[] { "return \"Hello world\"; " });

            var result = oh.onCall("test", rs, new string[] { "" });

            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void Reply_RS_Instance()
        {
            var rs = new RiveScript();
            var oh = new CSharp();

            oh.onLoad("test", new string[] { "return rs.GetHashCode().ToString(); " });

            var result = oh.onCall("test", rs, new string[] { "" });

            Assert.AreEqual(rs.GetHashCode().ToString(), result);
        }

        [TestMethod]
        public void Reply_Concatenet_Args_Id()
        {
            var rs = new RiveScript();
            var oh = new CSharp();

            oh.onLoad("test", new string[] { "return String.Join(\",\", args); " });

            var result = oh.onCall("test", rs, new string[] { "1", "2", "3" });

            Assert.AreEqual("1,2,3", result);
        }

        [TestMethod]
        public void Reply_CurrentUser_Not_Initialized()
        {
            var rs = new RiveScript();
            var oh = new CSharp();

            oh.onLoad("test", new string[] { "return rs.currentUser();" });

            var result = oh.onCall("test", rs, new string[] { "" });

            Assert.AreEqual(Constants.Undefined, result);
        }

        [TestMethod]
        public void Execute_Full_Object_Call_Without_Parameter()
        {
            var rs = new RiveScript();
            rs.stream(new[] { "",
                              "+ who is current user",
                              "- current user is: <call>current</call>",
                              "",
                              "> object current csharp",
                              "    return rs.currentUser();",
                              "< object",
                              ""
            });

            rs.sortReplies();

            var result = rs.reply("who is current user");

            Assert.AreEqual("current user is: default", result);
        }

        [TestMethod]
        public void Execute_Full_Object_Call_With_Parameter()
        {
            var rs = new RiveScript();
            rs.stream(new[] { "",
                              "+ sum # and #",
                              "- result is: <call>sum <star1> <star2></call>",
                              "",
                              "> object sum csharp",
                              "    return (int.Parse(args[0]) + int.Parse(args[1])).ToString();",
                              "< object",
                              ""
            });

            rs.sortReplies();

            var result = rs.reply("sum 10 and 15");

            Assert.AreEqual("result is: 25", result);
        }

        [TestMethod]
        public void Execute_Full_Object_Call_Custom_References()
        {
            var rs = new RiveScript();
            rs.stream(new[] { "",
                              "+ show data enum",
                              "- enum is: <call>test</call>",
                              "",
                              "> object test csharp",
                              "    reference System.Data.dll;",
                              "    using System.Data;",
                              "",
                              "    return DataRowState.Added.ToString();",
                              "< object",
                              ""
            });


            rs.sortReplies();

            var result = rs.reply("show data enum");

            Assert.AreEqual("enum is: Added", result);
        }


        [TestMethod]
        public void Execute_Object_Call_Entry_Assembly_Without_Explicit_Reference()
        {
            //Mock entry assembly for test envirioment
            ContextHelper.SetEntryAssembly(typeof(CSharpObjectHandlerTest).Assembly);

            var rs = new RiveScript();
            rs.stream(new[] { "",
                              "",
                              "",
                              "+ show context data",
                              "- data is: <call>context</call>",
                              "",
                              "> object context csharp",
                              "",
                              "    using RiveScript.Tests.Helper;",
                              "",
                              "    return ContextHelper.Property;",
                              "< object",
                              ""
            });

            rs.sortReplies();


            rs.reply("show context data")
              .AssertAreEqual("data is: Context_Property");
        }
    }
}
