using Renegade.Bridge.Interface;

namespace Renegade.Bridge.Model
{
    public class PendingMessage : IPendingMessage
    {
        public ulong? MessageId { get; init; }
        public string Content { get; init; }
        public IBridgeAuthor BridgeAuthor { get; init; }

        public string Channel { get; init; }

        public string WebHookUrl { get; init; }
    }
}
