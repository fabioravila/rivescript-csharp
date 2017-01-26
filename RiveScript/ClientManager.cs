using System;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace RiveScript
{
    /// <summary>
    ///  Manager for all the bot's users.
    /// </summary>
    public class ClientManager
    {
        private ConcurrentDictionary<string, Client> clients = new ConcurrentDictionary<string, Client>();

        public ClientManager() { }

        public string[] listClients()
        {
            return clients.Keys.ToArray();
        }

        public Client client(string username)
        {
            return clients.GetOrAdd(username, new Client(username));
        }

        public bool clientExists(string username)
        {
            return clients.ContainsKey(username);
        }
    }
}
