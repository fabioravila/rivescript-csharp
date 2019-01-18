using System.Collections.Generic;

namespace RiveScript.Session
{
    /// <summary>
    ///  Container for user variables.
    /// </summary>
    public class UserData
    {
        public string username { get; private set; }
        IDictionary<string, string> variables = new Dictionary<string, string>();
        public History history { get; private set; } = new History();
        public string lastMatch { get; set; }

        public UserData(string username)
        {
            this.username = username;
        }

        public void setVariable(string name, string value) => variables.AddOrUpdate(name, value);
        public bool removeVariable(string name) => variables.Remove(name);
        public string getVariable(string name) => variables.GetOrDefault(name, Constants.Undefined);

        public IDictionary<string, string> getVariables() => variables;
        public History getHistory() => history;
    }
}
