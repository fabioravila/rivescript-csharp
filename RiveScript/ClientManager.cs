using System;
using System.Collections.Generic;


namespace RiveScript
{
    /// <summary>
    ///  Manager for all the bot's users.
    /// </summary>
    public class ClientManager
    {
        private Dictionary<string, Client> clients = new Dictionary<string, Client>();

        public ClientManager() { }

        public string[] listClients()
        {
            return clients.Keys.ToArray();
        }

        public Client client(string username)
        {
            if (false == clients.ContainsKey(username))
            {
                clients.Add(username, new Client(username));
            }

            return clients[username];
        }

        public bool clientExists(string username)
        {
            return clients.ContainsKey(username);
        }
    }
}
