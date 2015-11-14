using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript
{
    public static class Util
    {
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
    }
}
