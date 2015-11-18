using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiveScript
{
    /// <summary>
    ///  Manager for all the bot's users.
    /// </summary>
    public class ClientManager
    {
        private Dictionary<string, Client> clients = new Dictionary<string, Client>();

        public ClientManager() { }

        public string[] Clients
        {
            get
            {
                return clients.Keys.ToArray();
            }
        }

        public Client Client(string username)
        {
            if (false == clients.ContainsKey(username))
            {
                clients.Add(username, new Client(username));
            }

            return clients[username];
        }

        public bool Exists(string username)
        {
            return clients.ContainsKey(username);
        }
    }
}
