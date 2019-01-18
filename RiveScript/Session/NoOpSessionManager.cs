using System.Collections.Generic;

namespace RiveScript.Session
{
    public class NoOpSessionManager : ISessionManager
    {
        static IDictionary<string, UserData> allEmpty = new Dictionary<string, UserData>();

        public void addHistory(string username, string input, string reply) { }

        public void clear(string username) { }

        public void clearAll() { }

        public void freeze(string username) { }

        public string get(string username, string name) => null;

        public UserData get(string username) => null;

        public IDictionary<string, UserData> getAll() => allEmpty;

        public History getHistory(string username) => null;

        public string getLastMatch(string username) => null;

        public UserData init(string username) => null;

        public void remove(string username, string name) { }

        public void set(string username, string name, string value) { }

        public void set(string username, IDictionary<string, string> vars) { }

        public void setLastMatch(string username, string trigger) { }

        public void thaw(string username, ThawAction action) { }
    }
}
