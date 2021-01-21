using System;

namespace Renegade.Bridge.Interface
{
    public interface IRecievedMessage
    {
        ulong MessageId { get; }
        DateTimeOffset TimeStamp { get; }

        string Channel { get; }

        IBridgeAuthor BridgeAuthor { get; }
        string Content { get; }
    }
}
