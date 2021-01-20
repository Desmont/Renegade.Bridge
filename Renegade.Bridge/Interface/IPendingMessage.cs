namespace Renegade.Bridge.Interface
{
    public interface IPendingMessage
    {
        string WebHookUrl { get; }
        string Channel { get; }

        IBridgeAuthor BridgeAuthor { get; }
        string Content { get; }
    }
}
