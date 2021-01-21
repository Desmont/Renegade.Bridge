using System;
using Renegade.Bridge.Interface;

namespace Renegade.Bridge.Model
{
    public class RecievedMessage : IRecievedMessage
    {
        public string AccountName { get; init; }
        public ulong Server { get; init; }
        public DateTimeOffset TimeStamp { get; init; }
        public ulong MessageId { get; init; }
        public string Channel { get; init; }
        public IBridgeAuthor BridgeAuthor { get; init; }
        public string Content { get; init; }
    }
}
