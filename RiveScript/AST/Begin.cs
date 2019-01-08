using System.Collections.Generic;

namespace RiveScript.AST
{
    /// <summary>
    /// Represents the "begin block" (configuration) data.
    /// </summary>
    public class Begin
    {
        public IDictionary<string, string> globals { get; set; } = new Dictionary<string, string>();                             // ! global
        public IDictionary<string, string> vars { get; set; } = new Dictionary<string, string>();                                // ! var
        public IDictionary<string, ICollection<string>> arrays { get; set; } = new Dictionary<string, ICollection<string>>();    // ! array
        public IDictionary<string, string> subs { get; set; } = new Dictionary<string, string>();                                // ! sub
        public IDictionary<string, string> person { get; set; } = new Dictionary<string, string>();                              // ! person

        public Begin() { }

        public void addGlobals(string name, string value)
        {
            globals.AddOrUpdate(name, value);
        }

        public void addVar(string name, string value)
        {
            vars.AddOrUpdate(name, value);
        }

        public void addSub(string name, string value)
        {
            subs.AddOrUpdate(name, value);
        }

        public void addPerson(string name, string value)
        {
            person.AddOrUpdate(name, value);
        }

        public void addArray(string name, ICollection<string> value)
        {
            arrays.AddOrUpdate(name, value);
        }

        public void removeArray(string name)
        {
            arrays.Remove(name);
        }
    }
}
