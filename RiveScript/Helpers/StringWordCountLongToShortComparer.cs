using System;

namespace RiveScript
{
    internal class StringWordCountLongToShortComparer : StringComparer
    {
        public override int Compare(string x, string y)
        {
            var x_count = Util.CountWords(x);
            var y_count = Util.CountWords(x);


            if (x_count < y_count)
            {
                return 1;
            }
            else if (x_count > y_count)
            {
                return -1;
            }
            else
                return 0;
        }

        public override bool Equals(string x, string y)
        {
            return Compare(x, y) == 0;
        }

        public override int GetHashCode(string obj)
        {
            return (obj ?? string.Empty).GetHashCode();
        }
    }
}
