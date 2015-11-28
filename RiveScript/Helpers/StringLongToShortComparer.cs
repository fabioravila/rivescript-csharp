using System;

namespace RiveScript
{
    internal class StringLongToShortComparer : StringComparer
    {
        public override int Compare(string x, string y)
        {
            //??
            //x.Length.CompareTo(y.Length);

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
            return (obj ?? string.Empty).GetHashCode();
        }
    }
}
