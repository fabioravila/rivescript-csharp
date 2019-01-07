using System.Collections.Generic;

namespace RiveScript.AST
{
    public class ObjectMacro
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public ICollection<string> code { get; set; }
    }
}
