namespace Renegade.Bridge.Interface
{
    public interface IBridgeAuthor
    {
        ulong AuthorId { get; }
        string Username { get; }
        string Avatar { get; }
    }
}
