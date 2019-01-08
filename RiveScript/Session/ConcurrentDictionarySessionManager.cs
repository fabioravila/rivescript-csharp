using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RiveScript.Session
{
    /// <summary>
    /// Implements the default in-memory session store for RiveScript, based on a <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>
    /// </summary>
    public class ConcurrentDictionarySessionManager : ISessionManager
    {
        ConcurrentDictionary<string, UserData> users;
        ConcurrentDictionary<string, UserData> frozen;

        public ConcurrentDictionarySessionManager()
        {
            users = new ConcurrentDictionary<string, UserData>();
            frozen = new ConcurrentDictionary<string, UserData>();
        }

        public void addHistory(string username, string input, string reply)
        {
            var userData = init(username);
            var history = userData.getHistory();

            history.addInput(input);
            history.addReply(reply);
        }

        public void clear(string username)
        {
            users.TryRemove(username, out UserData _dumb);
            frozen.TryRemove(username, out _dumb);
        }

        public void clearAll()
        {
            users.Clear();
            frozen.Clear();
        }

        public void freeze(string username)
        {
            var userData = users.GetOrDefault(username);
            if (userData == null)
                return;

            frozen.AddOrUpdate(username, CloneUserData(userData));
        }

        public string get(string username, string name)
        {
            var value = users.GetOrDefault(username);

            if (value == null)
                return null;

            return value.getVariable(name);
        }


        public void remove(string username, string name)
        {
            var value = users.GetOrDefault(username);
            if (value == null)
                return;

            value.removeVariable(name);
        }

        public UserData get(string username)
        {
            return users.GetOrDefault(username);
        }

        public IDictionary<string, UserData> getAll()
        {
            return users;
        }

        public History getHistory(string username)
        {
            var value = users.GetOrDefault(username);

            if (value == null)
                return null;

            return value.getHistory();
        }

        public string getLastMatch(string username)
        {
            var userData = init(username);
            return userData.lastMatch;
        }

        public UserData init(string username)
        {
            return users.GetOrAdd(username, key => CreateDefaultSession(key));
        }

        public void set(string username, string name, string value)
        {
            var userData = init(username);
            userData.setVariable(name, value);
        }

        public void set(string username, IDictionary<string, string> vars)
        {
            var userData = init(username);
            foreach (var item in vars)
            {
                userData.setVariable(item.Key, item.Value);
            }
        }

        public void setLastMatch(string username, string trigger)
        {
            var userData = init(username);
            userData.lastMatch = trigger;
        }

        public void thaw(string username, ThawAction action)
        {
            var frozenData = frozen.GetOrDefault(username);
            if (frozenData == null)
                return;

            switch (action)
            {
                case ThawAction.THAW:
                    users.AddOrUpdate(username, CloneUserData(frozenData)); //need clone??
                    frozen.TryRemove(username, out frozenData);
                    break;
                case ThawAction.DISCARD:
                    frozen.TryRemove(username, out frozenData);
                    break;
                case ThawAction.KEEP:
                    users.AddOrUpdate(username, CloneUserData(frozenData)); //here need clone!
                    break;
                default:
                    //NOTHING
                    break;
            }
        }

        UserData CreateDefaultSession(string username)
        {
            var data = new UserData(username);
            data.setVariable("topic", "random");
            data.lastMatch = "";
            return data;
        }

        UserData CloneUserData(UserData original)
        {
            var clone = CreateDefaultSession(original.username);

            //Copy user variables
            foreach (var item in original.getVariables())
            {
                clone.setVariable(item.Key, item.Value);
            }

            //Copy history
            var clone_hist = clone.getHistory();
            var origi_hist = original.getHistory();
            for (int i = 0; i < Constants.HISTORY_SIZE; i++)
            {
                clone_hist.input[i] = origi_hist.input[i];
                clone_hist.reply[i] = origi_hist.reply[i];
            }

            return clone;
        }

    }
}
