using System.Collections.Generic;

namespace RiveScript.AST
{
    /// <summary>
    /// Represents a RiveScript Object Macro.
    /// </summary>
    public class ObjectMacro
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public ICollection<string> Code { get; set; }
    }
}
