
using System;

namespace RiveScript
{
    public class ConcatMode
    {
        public string ConcatChar { get; }
        public string Name { get; set; }

        ConcatMode(string name, string concatChar)
        {
            Name = name;
            ConcatChar = concatChar;
        }

        public static ConcatMode NONE { get; } = new ConcatMode("NONE", "");
        public static ConcatMode NEWLINE { get; } = new ConcatMode("NEWLINE", "\n");
        public static ConcatMode SPACE { get; } = new ConcatMode("SPACE", " ");

        public static ConcatMode FromName(string name)
        {
            switch ((name ?? "").ToLower())
            {
                case "none":
                    return NONE;
                case "newline":
                    return NEWLINE;
                case "space":
                    return SPACE;
                default:
                    return null;
            }
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is ConcatMode && Equals((ConcatMode)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ConcatChar.GetHashCode() ^ Name.GetHashCode();
            }
        }

        public static bool operator ==(ConcatMode a, ConcatMode b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            return a.Equals(b);
        }

        public static bool operator !=(ConcatMode a, ConcatMode b)
        {
            return !(a == b);
        }
    }
}

