using System;

namespace Renegade.Bridge.Interface
{
    public interface IRecievedMessage
    {
        string AccountName { get; }

        ulong MessageId { get; }
        DateTimeOffset TimeStamp { get; }

        ulong Server { get; }
        string Channel { get; }

        IBridgeAuthor BridgeAuthor { get; }
        string Content { get; }
    }
}
