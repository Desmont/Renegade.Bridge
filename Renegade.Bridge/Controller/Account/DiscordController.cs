using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        private static readonly Regex s_webhookUrlRegex =
            new(@"^.*(discord|discordapp)\.com\/api\/webhooks\/([\d]+)\/([a-z0-9_-]+)$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly DiscordSocketClient _client;
        private readonly ILogger _discordLogger;
        private readonly AccountOptions _options;
        private readonly IDictionary<string, DiscordWebhookClient> _webhooks;


        public DiscordController(AccountOptions options, ILoggerFactory loggerFactory)
        {
            _options = options;
            _webhooks = new Dictionary<string, DiscordWebhookClient>();

            _discordLogger = loggerFactory.CreateLogger($"account.{Name}");

            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += MessageReceived;
            _client.MessageUpdated += MessageUpdated;
            _client.MessageDeleted += MessageDeleted;
            _client.LoginAsync(TokenType.Bot, _options.Token).Wait();
            _client.StartAsync().Wait();
        }

        public event Action<IRecievedMessage> Received;
        public event Action<IRecievedMessage> Updated;
        public event Action<ulong> Deleted;

        public string Name => _options.Name;

        public async Task<ulong?> SendAsync(IPendingMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.WebHookUrl)) throw new NotSupportedException();

            var webHook = GetWebHook(message.WebHookUrl);

            var messageId = await webHook.SendMessageAsync(message.Content, username: message.BridgeAuthor.Username,
                avatarUrl: message.BridgeAuthor.Avatar);

            return messageId;
        }

        public async Task UpdateAsync(IPendingMessage message)
        {
            if (message.MessageId == null) throw new ArgumentNullException(nameof(message.MessageId));
            if (string.IsNullOrWhiteSpace(message.WebHookUrl)) throw new NotSupportedException();

            var webHook = GetWebHook(message.WebHookUrl);

            await webHook.EditMessageAsync(message.MessageId.Value, message.Content);
        }

        public async Task DeleteAsync(IPendingMessage message)
        {
            if (message.MessageId == null) throw new ArgumentNullException(nameof(message.MessageId));
            if (string.IsNullOrWhiteSpace(message.WebHookUrl)) throw new NotSupportedException();

            var webHook = GetWebHook(message.WebHookUrl);

            await webHook.DeleteMessageAsync(message.MessageId.Value);
            
            // var guild = _client.GetGuild(ulong.Parse(_options.Server));
            // var channel = guild.Channels.First(x => x.Name == message.Channel);
            //
            // if (!(channel is IMessageChannel msgChannel))
            // {
            //     return false;
            // }
            //
            // await msgChannel.DeleteMessageAsync(message.MessageId.Value);
            //
            // return true;
        }

        private Task MessageReceived(SocketMessage message)
        {
            if (ShouldIgnoreAuthor(message.Author))
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
            if (ShouldIgnoreAuthor(message.Author))
            {
                return Task.CompletedTask;
            }

            Updated?.Invoke(ConvertMessage(message));

            return Task.CompletedTask;
        }

        private Task MessageDeleted(Cacheable<IMessage, ulong> oldMessage, ISocketMessageChannel channel)
        {
            if (oldMessage.HasValue && ShouldIgnoreAuthor(oldMessage.Value.Author))
            {
                return Task.CompletedTask;
            }

            Deleted?.Invoke(oldMessage.Id);

            return Task.CompletedTask;
        }

        private bool ShouldIgnoreAuthor(IUser author)
        {
            return author.Id == _client.CurrentUser.Id || author.IsWebhook;
        }

        private DiscordWebhookClient GetWebHook(string webHookUrl)
        {
            if (_webhooks.ContainsKey(webHookUrl))
            {
                return _webhooks[webHookUrl];
            }

            DiscordWebhookClient webHook;

            #region Discord.Net.Webhook 2.2.0 workaround

            // Discord.Net.Webhook 2.2.0 cant parse current discord webhook urls - do it manually
            var match = s_webhookUrlRegex.Match(webHookUrl);
            if (!match.Success)
            {
                throw new ArgumentNullException(nameof(webHookUrl), "The webhook token could not be parsed.");
            }

            // ensure that the first group is a ulong, set the _webhookId
            // 0th group is always the entire match, and 1 is the domain; so start at index 2
            if (!(match.Groups[2].Success && ulong.TryParse(match.Groups[2].Value, NumberStyles.None,
                CultureInfo.InvariantCulture, out var webhookId)))
            {
                throw new ArgumentNullException(nameof(webHookUrl),
                    "The webhook MessageId could not be parsed.");
            }

            if (!match.Groups[3].Success)
            {
                throw new ArgumentNullException(nameof(webHookUrl),
                    "The webhook token could not be parsed.");
            }

            var webhookToken = match.Groups[3].Value;
            webHook = new DiscordWebhookClient(webhookId, webhookToken);

            #endregion

            // webHook = new DiscordWebhookClient(webHookUrl);
            webHook.Log += Log;

            _webhooks.Add(webHookUrl, webHook);

            return webHook;
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
