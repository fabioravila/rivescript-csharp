using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace RiveScript
{
    internal static class Util
    {
        /// <summary>
        /// Shift an item to the beginning of an array and rotate.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addition"></param>
        /// <returns></returns>
        public static string[] Unshift(string[] array, string addition)
        {
            //TODO: Use Array.Copy
            for (int i = array.Length - 1; i > 0; i--)
            {
                array[i] = array[i - 1];
            }

            array[0] = addition;
            return array;
        }

        /// <summary>
        /// Shift an item to the beginning of an array and rotate.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="addition"></param>
        /// <returns></returns>
        public static List<string> Unshift(List<string> list, string addition)
        {
            //TODO: Use Array.Copy
            for (int i = list.Count - 1; i > 0; i--)
            {
                list[i] = list[i - 1];
            }

            list[0] = addition;
            return list;
        }



        /// <summary>
        /// Sort the integer keys in a Dictionary from highest to lowest.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static int[] SortKeysDesc(IDictionary<int, ICollection<string>> hash)
        {
            //Get all keys
            var keys = hash.Keys.ToArray();

            //Sors keys in ASC
            Array.Sort(keys);

            //Reverse array
            Array.Reverse(keys);

            return keys;
        }

        /// <summary>
        /// Sort strings in a list by length from longest to shortest.
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string[] SortByLengthDesc(string[] list)
        {
            Array.Sort(list, new StringLengthLongToShortComparer());
            return list;
        }

        public static string Join(string[] value, string separator)
        {
            if (value == null)
                return null;

            if (separator == null)
                separator = "";

            return string.Join(separator, value);
        }

        public static string[] CopyOfRange(string[] source, int from, int to)
        {
            int len = to - from;
            string[] dest = new string[len];
            Array.Copy(source, from, dest, 0, len);
            return dest;
        }

        public static MatchCollection GetRegexMatches(string pattern, string input)
        {
            return new Regex(pattern).Matches(input);
        }

        public static MatchCollection GetRegexMatches(Regex regex, string input)
        {
            return regex.Matches(input);
        }

        public static bool IsTrue(string value)
        {
            return value == "true" || value == "1" || value == "yes";
        }

        public static bool IsFalse(string value)
        {
            return value == "false" || value == "0" || value == "no";
        }

        public static string Strip(string line)
        {
            if (line == null)
                return null;

            //Note: This will keep the \s \n \t on end of string, but on start this will be trimmed.
            //Keep that in mind when do continuations

            line = line.ReplaceRegex(@"\s+\r\n\t", " ");
            line = line.TrimStart().TrimEnd(' ');

            return line;
        }

        public static string StripNasties(string line, bool utf8)
        {
            if (utf8)//Allow most things in UTF8 mode.
            {
                return line.ReplaceRegex("[\\<>]+", "");
            }
            else
            {
                return line.ReplaceRegex("[^A-Za-z0-9 ]", "");
            }
        }

        public static int CountWords(string source)
        {
            //a fast way to do this
            int count = 0;
            bool inWord = false;

            foreach (char t in source)
            {
                if (char.IsWhiteSpace(t))
                {
                    inWord = false;
                }
                else
                {
                    if (!inWord) count++;
                    inWord = true;
                }
            }
            return count;
        }
    }
}

