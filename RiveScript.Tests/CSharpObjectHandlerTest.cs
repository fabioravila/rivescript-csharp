﻿using System;
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
            rs.setHandler("csharp", new CSharp());
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
            rs.setHandler("csharp", new CSharp());
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
            rs.setHandler("csharp", new CSharp());
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

    }
}