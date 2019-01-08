using System.Collections.Generic;

namespace RiveScript.AST
{
    /// <summary>
    /// Represents the "begin block" (configuration) data.
    /// </summary>
    public class Begin
    {
        IDictionary<string, string> globals { get; set; } = new Dictionary<string, string>();                             // ! global
        IDictionary<string, string> vars { get; set; } = new Dictionary<string, string>();                                // ! var
        IDictionary<string, ICollection<string>> arrays { get; set; } = new Dictionary<string, ICollection<string>>();    // ! array
        IDictionary<string, string> subs { get; set; } = new Dictionary<string, string>();                                // ! sub
        IDictionary<string, string> person { get; set; } = new Dictionary<string, string>();                              // ! person

        public Begin() { }

        public void addGlobals(string name, string value)
        {
            globals.Add(name, value);
        }

        public void addVar(string name, string value)
        {
            vars.Add(name, value);
        }

        public void addSub(string name, string value)
        {
            subs.Add(name, value);
        }

        public void addPerson(string name, string value)
        {
            person.Add(name, value);
        }

        public void addArray(string name, ICollection<string> value)
        {
            arrays.Add(name, value);
        }

        public void removeArray(string name)
        {
            arrays.Remove(name);
        }
    }
}
