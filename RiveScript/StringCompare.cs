using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript
{
    public class StringCompare : StringComparer
    {
        public override int Compare(string x, string y)
        {
            if (x.Length < y.Length)
            {
                return 1;
            }
            else if (x.Length > y.Length)
            {
                return -1;
            }
            else
                return 0;
        }

        public override bool Equals(string x, string y)
        {
            return this.Compare(x, y) == 0;
        }

        public override int GetHashCode(string obj)
        {
            throw new NotImplementedException();
        }
    }
}
