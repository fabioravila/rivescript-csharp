using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.Session
{
    public interface ISessionManager
    {
        /**
         * Makes sure a username has a session (creates one if not).
         *
         * @param username the username
         * @return the user data
         */
        UserData init(string username);

        /**
         * Sets a user's variable.
         *
         * @param username the username
         * @param name     the variable name
         * @param value    the variable value
         */
        void set(string username, string name, string value);

        /**
         * Sets a user's variables.
         *
         * @param username the username
         * @param vars     the user variables
         */
        void set(string username, IDictionary<string, string> vars);

        /**
         * Adds input and reply to a user's history.
         *
         * @param username the username
         * @param input    the input
         * @param reply    the reply
         */
        void addHistory(string username, string input, string reply);

        /**
         * Sets a user's the last matched trigger.
         *
         * @param username the username
         * @param trigger  the trigger
         */
        void setLastMatch(string username, string trigger);

        /**
         * Returns a user variable.
         *
         * @param username the username
         * @param name     the variable name
         * @return the variable value
         */
        string get(string username, string name);

        /**
         * Returns all variables for a user.
         *
         * @param username the username
         * @return the user data
         */
        UserData get(string username);

        /**
         * Returns all variables about all users.
         *
         * @return the users and their user data
         */
        IDictionary<string, UserData> getAll();

        /**
         * Returns a user's last matched trigger.
         *
         * @param username the username
         * @return the last matched trigger
         */
        string getLastMatch(string username);

        /**
         * Returns a user's history.
         *
         * @param username the username
         * @return the history
         */
        History getHistory(string username);

        /**
         * Clears a user's variables.
         *
         * @param username the username
         */
        void clear(string username);

        /**
         * Clear all variables of all users.
         */
        void clearAll();

        /**
         * Makes a snapshot of a user's variables.
         *
         * @param username the username
         */
        void freeze(string username);

        /**
         * Unfreezes a user's variables.
         *
         * @param username the username
         * @param action   the thaw action
         * @see ThawAction
         */
        void thaw(string username, ThawAction action);
    }
}
