
using System;

namespace RiveScript
{
    [Obsolete("No need to use this", true)]
    internal static class Rot13
    {
        /// <summary>
        /// Performs the ROT13 character rotation.
        /// </summary>
        public static string Transform(string value)
        {
            char[] array = value.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                int c = (int)array[i];

                if ((c >= 'a' && c <= 'm') || (c >= 'A' && c <= 'M'))
                {
                    c += 13;
                }
                else if ((c >= 'n' && c <= 'z') || (c >= 'N' && c <= 'Z'))
                {
                    c -= 13;
                }

                array[i] = (char)c;
            }

            return new string(array);
        }
    }
}
