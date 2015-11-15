using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RiveScript
{
    public static class Util
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
        public static string Subistitute(string[] sorted, IDictionary<string, string> hash, string text)
        {
            for (int i = 0; i < sorted.Length; i++)
            {
                var pattern = sorted[i];
                var result = hash[sorted[i]];
                var rot13 = Rot13.Transform(result);

                var quotemeta = @pattern;

                text = Regex.Replace(text, ("^" + quotemeta + "$"), ("<rot13sub>" + rot13 + "<bus31tor>"));
                text = Regex.Replace(text, ("^" + quotemeta + "(\\W+)"), ("<rot13sub>" + rot13 + "<bus31tor>$1"));
                text = Regex.Replace(text, ("(\\W+)" + quotemeta + "(\\W+)"), ("$1<rot13sub>" + rot13 + "<bus31tor>$2"));
                text = Regex.Replace(text, ("(\\W+)" + quotemeta + "$"), ("$1<rot13sub>" + rot13 + "<bus31tor>"));
            }

            if (text.IndexOf("<rot13sub>") > -1)
            {
                var re = new Regex("<rot13sub>(.+?)<bus31tor>");
                var mc = re.Matches(text);
                foreach (Match m in mc)
                {
                    var block = m.Groups[0].Value;
                    var data = Rot13.Transform(m.Groups[1].Value);
                    text = text.Replace(block, data);
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

    }
}
