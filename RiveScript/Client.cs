using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript
{
    /// <summary>
    /// Represent individual user´s data
    /// </summary>
    public class Client
    {
        private string id;
        private IDictionary<string, string> data = new Dictionary<string, string>();
        private string[] input = new string[10];
        private string[] reply = new string[10];

        /// <summary>
        /// Create a new client instance
        /// </summary>
        /// <param name="id">Unique ID for this client</param>
        public Client(string id)
        {
            this.id = id;




            //Initial user´s hitory
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = Constants.Undefined;
                reply[i] = Constants.Undefined;
            }

        }

        /// <summary>
        /// Set a variable for the client
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Set(string name, string value)
        {
            data.Add(name, value);
        }

        /// <summary>
        /// Get a variable from client. Return undefined if doens´t exist
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Get(string name)
        {
            if (data.ContainsKey(name))
                return data[name];

            return Constants.Undefined;
        }

        /// <summary>
        /// Delete a variable on the client
        /// </summary>
        /// <param name="name"></param>
        public void Delete(string name)
        {
            if (data.ContainsKey(name))
                data.Remove(name);
        }

        /// <summary>
        /// Retrieve a dictionary of all user´s data
        /// </summary>
        public IDictionary<string, string> Data
        {
            get
            {
                return this.data;
            }
        }

        /// <summary>
        /// Replace all internal user data with new data (dangerous!)
        /// </summary>
        /// <param name="newData"></param>
        /// <returns></returns>
        public bool ReplaceData(IDictionary<string, string> newData)
        {
            this.data = newData;
            return true;
        }

        /// <summary>
        /// Add a line to the user´s input history
        /// </summary>
        /// <param name="text"></param>
        public void AddInput(string text)
        {
            input = Util.Unshift(input, text);
        }

        /// <summary>
        /// Add a line to the user´s reply history
        /// </summary>
        /// <param name="text"></param>
        public void AddReply(string text)
        {
            reply = Util.Unshift(reply, text);
        }

        /// <summary>
        /// Get specific input value by index
        /// </summary>
        /// <param name="index">The index of the input value to get (1-9)</param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <returns></returns>
        public string GetInput(int index)
        {
            if (index >= 1 && index <= input.Length - 1)
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
        public string GetReply(int index)
        {
            if (index >= 1 && index <= reply.Length - 1)
            {
                return reply[index - 1];
            }

            throw new IndexOutOfRangeException();
        }
    }
}
