using System;
using Renegade.Bridge.Interface;
using Renegade.Bridge.Model;
using Renegade.Bridge.Model.Configuration;

namespace Renegade.Bridge.Controller
{
    public class ClientController
    {
        private readonly ClientOptions _options;

        public ClientController(IAccountController account, ClientOptions options)
        {
            _options = options;

            Account = account;
            Account.Received += OnReceived;
            Account.Updated += OnUpdated;
            Account.Deleted += OnDeleted;
        }

        public IAccountController Account { get; }

        public event Action<ClientController, IRecievedMessage> Received;
        public event Action<ClientController, IRecievedMessage> Updated;
        public event Action<ClientController, ulong> Deleted;

        private void OnReceived(IRecievedMessage message)
        {
            if (message.Channel != _options.Channel)
            {
                return;
            }

            Received?.Invoke(this, message);
        }

        private void OnUpdated(IRecievedMessage message)
        {
            if (message.Channel != _options.Channel)
            {
                return;
            }

            Updated?.Invoke(this, message);
        }

        private void OnDeleted(ulong id)
        {
            Deleted?.Invoke(this, id);
        }

        public ulong Send(IRecievedMessage message)
        {
            return Account.Send(ConvertMessage(message)).Result;
        }

        public ulong Update(IRecievedMessage message, ulong messageId)
        {
            return Account.Update(ConvertMessage(message, messageId)).Result;
        }
        
        public void Delete(ulong messageId)
        {
            Account.Delete(messageId).Wait();
        }

        private IPendingMessage ConvertMessage(IRecievedMessage message, ulong? messageId = null)
        {
            return new PendingMessage
            {
                MessageId = messageId,
                Content = message.Content,
                BridgeAuthor = message.BridgeAuthor,
                Channel = _options.Channel,
                WebHookUrl = _options.WebHookUrl
            };
        }

    }
}
