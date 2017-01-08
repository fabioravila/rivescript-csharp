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
        /// Run a substitutions on a string
        /// </summary>
        /// <param name="sorted"> The sorted list os substitutions patterns to process</param>
        /// <param name="hash">A hash that pairs the sorted list with the replacement texts</param>
        /// <param name="text">Text to apply the substitution</param>
        /// <returns></returns>
        public static string Substitute(string[] sorted, IDictionary<string, string> hash, string text)
        {
            var ph = new List<string>();
            var pi = 0;

            for (int i = 0; i < sorted.Length; i++)
            {
                var pattern = sorted[i];
                var result = hash[sorted[i]];
                //var rot13 = Rot13.Transform(result);
                ph.Add(result);
                var placeholder = "<rot13sub>" + pi + "<bus31tor>";
                pi++;

                //Original JavaCode: var quotemeta = @pattern;
                //Run escape make sure no conflict like * in substitution
                var quotemeta = Regex.Escape(@pattern);

                text = text.ReplaceRegex(("^" + quotemeta + "$"), placeholder);
                text = text.ReplaceRegex(("^" + quotemeta + "(\\W+)"), ("" + placeholder + "$1"));
                text = text.ReplaceRegex(("(\\W+)" + quotemeta + "(\\W+)"), ("$1" + placeholder + "$2"));
                text = text.ReplaceRegex(("(\\W+)" + quotemeta + "$"), ("$1" + placeholder + ""));
            }

            var tries = 0;
            while (text.IndexOf("<rot13sub>") > -1)
            {
                tries++;
                if (tries > 50)
                {
                    Console.WriteLine("[RS] Too many loops in substitution placeholders!");
                    break;
                }

                var re = new Regex("<rot13sub>(.+?)<bus31tor>");
                var mc = re.Matches(text);
                foreach (Match m in mc)
                {
                    var block = m.Groups[0].Value;
                    var idx = int.Parse(m.Groups[1].Value);
                    text = text.Replace(block, ph[idx]);
                }
            }

            return text;
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
            Array.Sort(list, new StringLongToShortComparer());
            return list;
        }


        public static string Join(string[] value, string separator)
        {
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


    }
}
