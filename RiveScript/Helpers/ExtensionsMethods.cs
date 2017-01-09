using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


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

        public static void AddOrUpdate(this IDictionary<string, string> dic, string key, string value)
        {
            if (dic.ContainsKey(key))
                dic[key] = value;
            else
                dic.Add(key, value);
        }

        public static string[] Split(this string @this, string pattern)
        {
            return @this.Split(new[] { pattern }, StringSplitOptions.None);
        }


        public static string[] Split(this string @this, string pattern, int count)
        {
            return @this.Split(new[] { pattern }, count, StringSplitOptions.None);
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

        public static bool MatchRegex(this string @this, string pattern)
        {
            return Regex.IsMatch(@this, pattern);
        }


        public static T[] ToArray<T>(this IEnumerable<T> collection)
        {
            //TODO: I can do better!
            var list = new List<T>();
            list.AddRange(collection);
            return list.ToArray();
        }

        public static T[] ToSubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] ToSubArray<T>(this T[] data, int index)
        {
            return data.ToSubArray(index, data.Length - index);
        }

        public static bool Contains(this string @this, string value)
        {
            if (string.IsNullOrWhiteSpace(@this))
                return false;

            return (@this.IndexOf(value) > -1);
        }

    }
}
