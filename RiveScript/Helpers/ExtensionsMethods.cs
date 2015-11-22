using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        //public static char ToLower(this char c)
        //{
        //    return c.

        //}
    }
}
