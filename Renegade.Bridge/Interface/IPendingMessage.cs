namespace Renegade.Bridge.Interface
{
    public interface IPendingMessage
    {
        ulong? MessageId { get; }

        string Channel { get; }

        IBridgeAuthor BridgeAuthor { get; }
        string Content { get; }
    }
}
