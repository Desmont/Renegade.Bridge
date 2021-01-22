using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Renegade.Bridge.Interface;
using Renegade.Bridge.Model;
using Renegade.Bridge.Model.Configuration;

namespace Renegade.Bridge.Controller.Account
{
    public class DiscordController : IAccountController
    {
        private readonly ILogger _discordLogger;
        private readonly AccountOptions _options;
        private readonly DiscordSocketClient _socketClient;
        private readonly IDictionary<string, DiscordWebhookClient> _webhookClients;

        public DiscordController(AccountOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;

            _webhookClients = new Dictionary<string, DiscordWebhookClient>();

            _discordLogger = loggerFactory.CreateLogger($"account.{Name}");

            _socketClient = new DiscordSocketClient();
            _socketClient.Log += Log;
            _socketClient.MessageReceived += MessageReceived;
            _socketClient.MessageUpdated += MessageUpdated;
            _socketClient.MessageDeleted += MessageDeleted;
            _socketClient.LoginAsync(TokenType.Bot, _options.Token).Wait();
            _socketClient.StartAsync().Wait();
        }

        public event Action<IRecievedMessage> Received;
        public event Action<IRecievedMessage> Updated;
        public event Action<ulong> Deleted;

        public string Name => _options.Name;

        public async Task<ulong?> SendAsync(IPendingMessage message)
        {
            var webHook = await GetWebHookClientAsync(message.Channel);

            var messageId = await webHook.SendMessageAsync(message.Content, username: message.BridgeAuthor.Username,
                avatarUrl: message.BridgeAuthor.Avatar);

            return messageId;
        }

        public async Task UpdateAsync(IPendingMessage message)
        {
            if (message.MessageId == null)
            {
                throw new ArgumentNullException(nameof(message.MessageId));
            }

            var webHook = await GetWebHookClientAsync(message.Channel);

            await webHook.EditMessageAsync(message.MessageId.Value, message.Content);
        }

        public async Task DeleteAsync(IPendingMessage message)
        {
            if (message.MessageId == null)
            {
                throw new ArgumentNullException(nameof(message.MessageId));
            }

            var webHook = await GetWebHookClientAsync(message.Channel);

            await webHook.DeleteMessageAsync(message.MessageId.Value);
        }

        private Task MessageReceived(SocketMessage message)
        {
            if (!CorrectMessage(message))
            {
                return Task.CompletedTask;
            }

            _discordLogger.Log(LogLevel.Debug, message.ToString());

            Received?.Invoke(ConvertMessage(message));

            return Task.CompletedTask;
        }

        private Task MessageUpdated(Cacheable<IMessage, ulong> oldMessage, SocketMessage message,
            ISocketMessageChannel channel)
        {
            if (!CorrectMessage(message))
            {
                return Task.CompletedTask;
            }

            Updated?.Invoke(ConvertMessage(message));

            return Task.CompletedTask;
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> oldMessage, ISocketMessageChannel channel)
        {
            if (oldMessage.HasValue && !CorrectMessage(oldMessage.Value))
            {
                return Task.CompletedTask;
            }

            Deleted?.Invoke(oldMessage.Id);

            return Task.CompletedTask;
        }

        private bool CorrectMessage(IMessage message)
        {
            return (message.Channel as SocketGuildChannel)?.Guild.Id == ulong.Parse(_options.Server) // guild
                   && message.Author.Id != _socketClient.CurrentUser.Id && !message.Author.IsWebhook; // author
        }

        private async Task<DiscordWebhookClient> GetWebHookClientAsync(string channelName)
        {
            if (_webhookClients.ContainsKey(channelName))
            {
                return _webhookClients[channelName];
            }

            var guild = _socketClient.GetGuild(ulong.Parse(_options.Server));
            if (!(guild.Channels.First(x => x.Name == channelName) is ITextChannel channel))
            {
                throw new NotSupportedException("Webhook requires ITextChannel");
            }

            var hook = (await channel.GetWebhooksAsync()).FirstOrDefault() ??
                       await channel.CreateWebhookAsync($"Renegade.Bridge:{channelName}");

            var webhookClient = new DiscordWebhookClient(hook);
            _webhookClients.Add(channelName, webhookClient);

            return webhookClient;
        }

        private IRecievedMessage ConvertMessage(IMessage message)
        {
            if (!(message.Channel is SocketGuildChannel channel))
            {
                return null;
            }

            var author = new BridgeAuthor
            {
                AuthorId = message.Author.Id,
                Avatar = message.Author.GetAvatarUrl(),
                Username = message.Author.Username
            };

            return new RecievedMessage
            {
                AccountName = Name,
                BridgeAuthor = author,
                MessageId = message.Id,
                TimeStamp = message.Timestamp,
                Channel = channel.Name,
                Server = channel.Guild.Id,
                Content = message.Content
            };
        }

        private Task Log(LogMessage message)
        {
            _discordLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
        {
            return (LogLevel)Math.Abs((int)severity - 5);
        }
    }
}
