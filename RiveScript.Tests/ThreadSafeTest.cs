using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace RiveScript.Tests
{
    /// <summary>
    /// Summary description for ThreadSafeTest
    /// </summary>
    [TestClass]
    public class ThreadSafeTest
    {
        [TestMethod]
        public void CurrentUser_Is_ThreadSafe_On_Macro()
        {

            var rs = new RiveScript(debug: true);
            rs.setCSharpHandler();

            rs.stream(@"+ trigger
                        - <call>currentuser</call>

                        > object currentuser csharp
                            return rs.currentUser();
                        < object");

            rs.sortReplies();


            Action action = () =>
            {
                var threadid = Thread.CurrentThread.ManagedThreadId.ToString();

                var reply = rs.reply(threadid, "trigger");

                Assert.AreEqual(threadid, reply);
            };


            Parallel.Invoke(action, action, action);

        }


        [TestMethod]
        public void ClientManager_Is_ThreadSafe()
        {

            var rs = new RiveScript(debug: true);
            rs.setCSharpHandler();
            rs.stream(@"+ trigger
                        - <call>currentuser</call>

                        > object currentuser csharp
                            return rs.currentUser();
                        < object");

            rs.sortReplies();

            Action action = () => rs.reply(Thread.CurrentThread.ManagedThreadId.ToString(), "trigger");


            Parallel.Invoke(action, action, action, action, action, action, action, action);

        }
    }
}
