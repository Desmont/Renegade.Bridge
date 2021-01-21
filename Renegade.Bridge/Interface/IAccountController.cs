using System;
using System.Threading.Tasks;

namespace Renegade.Bridge.Interface
{
    public interface IAccountController
    {
        string Name { get; }
        
        Task<ulong?> SendAsync(IPendingMessage message);
        Task UpdateAsync(IPendingMessage message);
        Task DeleteAsync(IPendingMessage message);

        event Action<IRecievedMessage> Received;
        event Action<IRecievedMessage> Updated;
        event Action<ulong> Deleted;
    }
}
