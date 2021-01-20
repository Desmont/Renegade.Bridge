using Renegade.Bridge.Interface;

namespace Renegade.Bridge.Model
{
    public class BridgeAuthor : IBridgeAuthor
    {
        public ulong AuthorId { get; init; }
        public string Username { get; init; }
        public string Avatar { get; init; }
    }
}
