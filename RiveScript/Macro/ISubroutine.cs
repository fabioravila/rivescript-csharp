using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript.Macro
{
    public interface ISubroutine
    {
        string Call(RiveScript rs, string[] args);
    }
}
