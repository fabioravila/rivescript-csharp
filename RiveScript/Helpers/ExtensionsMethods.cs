using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RiveScript
{
    /// <summary>
    /// Helper methods to do thins more fast
    /// </summary>
    internal static class ExtensionsMethods
    {

        public static void AddRange<T>(this ICollection<T> collection, T[] itens)
        {
            foreach (var item in itens)
            {
                collection.Add(item);
            }
        }


        public static string[] Split(this string @this, string pattern)
        {
            return @this.Split(new[] { pattern }, StringSplitOptions.None);
        }


        public static string[] SplitRegex(this string @this, string pattern)
        {
            return new Regex(pattern).Split(@this);
        }


        public static string[] SplitRegex(this string @this, string pattern, int count)
        {
            return new Regex(pattern).Split(@this, count);
        }

        public static string[] SplitRegex(this string @this, string pattern, int count, int startat)
        {
            return new Regex(pattern).Split(@this, count, startat);
        }


        public static string ReplaceRegex(this string @this, string pattern, string replacement)
        {
            return new Regex(pattern).Replace(@this, replacement);
        }

        public static string ReplaceRegex(this string @this, string pattern, string replacement, int count)
        {
            return new Regex(pattern).Replace(@this, replacement, count);
        }

        public static string ReplaceRegex(this string @this, string pattern, string replacement, int count, int startat)
        {
            return new Regex(pattern).Replace(@this, replacement, count, startat);
        }

        //public static char ToLower(this char c)
        //{
        //    return c.

        //}
    }
}
