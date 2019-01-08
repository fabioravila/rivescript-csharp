using System;
using System.Collections.Generic;

namespace RiveScript.Session
{
    public class History
    {
        public List<string> input { get; private set; }
        public List<string> reply { get; private set; }

        public History()
        {
            input = new List<string>();
            reply = new List<string>();

            for (int i = 0; i < Constants.HISTORY_SIZE; i++)
            {
                input.Add(Constants.Undefined);
                reply.Add(Constants.Undefined);
            }
        }


        /// <summary>
        /// Add a line to the user´s input history
        /// </summary>
        /// <param name="text"></param>
        public void addInput(string text)
        {
            input = Util.Unshift(input, text);
        }

        /// <summary>
        /// Add a line to the user´s reply history
        /// </summary>
        /// <param name="text"></param>
        public void addReply(string text)
        {
            reply = Util.Unshift(reply, text);
        }

        /// <summary>
        /// Get specific input value by index
        /// </summary>
        /// <param name="index">The index of the input value to get (1-9)</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <returns></returns>
        public string getInput(int index)
        {
            if (index >= 1 && index <= input.Count - 1)
            {
                return input[index - 1];
            }

            throw new IndexOutOfRangeException();
        }

        /// <summary>
        /// Get specific reply value by index
        /// </summary>
        /// <param name="index">The index of the reply value to get (1-9)</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <returns></returns>
        public string getReply(int index)
        {
            if (index >= 1 && index <= reply.Count - 1)
            {
                return reply[index - 1];
            }

            throw new IndexOutOfRangeException();
        }

    }
}
