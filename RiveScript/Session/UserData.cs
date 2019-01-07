using System.Collections.Generic;

namespace RiveScript.Session
{
    public class UserData
    {
        IDictionary<string, string> variables = new Dictionary<string, string>();
        public History history { get; private set; } = new History();
        public string lastMatch { get; set; }

        public UserData() { }

        public void setVariable(string name, string value)
        {
            variables.AddOrUpdate(name, value);
        }

        public string getVariable(string name) => variables.GetOrDefault(name);
        public IDictionary<string, string> getVariables() => variables;
        public History getHistory() => history;
    }
}
