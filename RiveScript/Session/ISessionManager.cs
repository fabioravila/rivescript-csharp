using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.Session
{
    public interface ISessionManager
    {
        /// <summary>
        /// Makes sure a username has a session (creates one if not).
        /// </summary>
        /// <param name="username">the username</param>
        /// <returns>the user data</returns>
        UserData init(string username);

        /// <summary>
        ///  Sets a user's variable.
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="name">the variable name</param>
        /// <param name="value">the variable value</param>
        void set(string username, string name, string value);

        /// <summary>
        /// Sets a user's variables.
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="vars">the user variables</param>
        void set(string username, IDictionary<string, string> vars);

        /// <summary>
        ///  Adds input and reply to a user's history.
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="input">the input</param>
        /// <param name="reply">the reply</param>
        void addHistory(string username, string input, string reply);


        /// <summary>
        /// Sets a user's the last matched trigger.
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="trigger">the trigger</param>
        void setLastMatch(string username, string trigger);

        /// <summary>
        /// Returns a user variable.
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="name">the variable name</param>
        /// <returns>the variable value</returns>
        string get(string username, string name);

        /// <summary>
        /// Remove a user variable.
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="name">the variable name</param>
        /// <returns>the variable value</returns>
        void remove(string username, string name);

        /// <summary>
        /// Returns all variables for a user.
        /// </summary>
        /// <param name="username">username the username</param>
        /// <returns>user data</returns>
        UserData get(string username);

        /// <summary>
        /// Returns all variables about all users.
        /// </summary>
        /// <returns>the users and their user data</returns>
        IDictionary<string, UserData> getAll();

        /// <summary>
        /// Returns a user's last matched trigger.
        /// </summary>
        /// <param name="username">the username</param>
        /// <returns>the last matched trigger</returns>
        string getLastMatch(string username);

        /// <summary>
        /// Returns a user's history.
        /// </summary>
        /// <param name="username">the username</param>
        /// <returns>the history</returns>
        History getHistory(string username);

        /// <summary>
        /// Clears a user's variables.
        /// </summary>
        /// <param name="username">the username</param>
        void clear(string username);

        /// <summary>
        /// Clear all variables of all users.
        /// </summary>
        void clearAll();

        /// <summary>
        /// Makes a snapshot of a user's variables.
        /// </summary>
        /// <param name="username"> the username</param>
        void freeze(string username);

        /// <summary>
        /// Unfreezes a user's variables.
        /// </summary>
        /// <param name="username">the username</param>
        /// <param name="action">the thaw action</param>
        void thaw(string username, ThawAction action);
    }
}
