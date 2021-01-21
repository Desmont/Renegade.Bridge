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
            var entry = new GatewayEntry { ClientName = sender.Name, Message = message};
            foreach (var client in Clients.Where(client => client != sender))
            {
                var messageId = client.Send(message);
                if (messageId == null) throw new Exception("Message not sent"); // TODO MKA logging
                
                entry.MessageIds.Add(new KeyValuePair<string, ulong>(client.Name,
                    messageId.Value));
            }

            _entries.Add(entry);
        }

        private void ClientOnUpdated(ClientController sender, IRecievedMessage message)
        {
            var entry = GetMessage(sender.Name, message.MessageId);
            if (entry == null)
            {
                return;
            }

            entry.Message = message;
            
            foreach (var (clientName, messageId) in entry.MessageIds)
            {
                var client = Clients.First(x => x.Name == clientName);

                client.Update(message, messageId);
            }
        }

        private void ClientOnDeleted(ClientController sender, ulong deletedMessageId)
        {
            var entry = GetMessage(sender.Name, deletedMessageId);
            if (entry == null)
            {
                return;
            }
            
            foreach (var (clientName, messageId) in entry.MessageIds)            {
                var client = Clients.First(x => x.Name == clientName);

                client.Delete(entry.Message, messageId);
            }
        }
        
        private GatewayEntry GetMessage(string clientName, ulong messageId)
        {
            return _entries.FirstOrDefault(x => x.ClientName == clientName && x.Message.MessageId == messageId);
        }
    }
}
