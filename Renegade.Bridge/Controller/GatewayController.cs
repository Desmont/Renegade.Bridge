using System;
using System.Collections.Generic;
using System.Linq;
using Renegade.Bridge.Interface;
using Renegade.Bridge.Model;

namespace Renegade.Bridge.Controller
{
    public class GatewayController : IGatewayController
    {
        private readonly IList<GatewayEntry> _entries = new List<GatewayEntry>();

        public GatewayController(string name, IList<ClientController> clients)
        {
            Name = name;
            Clients = clients;

            foreach (var client in Clients)
            {
                client.Received += ClientOnReceived;
                client.Updated += ClientOnUpdated;
                client.Deleted += ClientOnDeleted;
            }
        }

        public string Name { get; }
        public IList<ClientController> Clients { get; }

        private void ClientOnReceived(ClientController sender, IRecievedMessage message)
        {
            var entry = new GatewayEntry {Message = message, AccountName = sender.Account.Name};
            foreach (var clientController in Clients.Where(x => x != sender))
            {
                entry.MessageIds.Add(new KeyValuePair<string, ulong>(clientController.Account.Name,
                    clientController.Send(message)));
            }

            _entries.Add(entry);
        }

        private void ClientOnUpdated(ClientController sender, IRecievedMessage message)
        {
            var entry = _entries.FirstOrDefault(x => x.Message.MessageId == message.MessageId);
            if (entry == null)
            {
                return;
            }

            foreach (var (accountName, messageId) in entry.MessageIds)
            {
                var clientController = Clients.First(x => x.Account.Name == accountName);

                clientController.Update(message, messageId);
            }
        }

        private void ClientOnDeleted(ClientController sender, ulong deletedMessageId)
        {
            var entry = _entries.FirstOrDefault(x => x.Message.MessageId == deletedMessageId);
            if (entry == null)
            {
                return;
            }
            
            foreach (var (accountName, messageId) in entry.MessageIds)
            {
                var clientController = Clients.First(x => x.Account.Name == accountName);

                clientController.Delete(messageId);
            }
        }
    }
}
