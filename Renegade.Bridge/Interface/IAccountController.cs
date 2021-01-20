using System;
using System.Threading.Tasks;

namespace Renegade.Bridge.Interface
{
    public interface IAccountController
    {
        string Name { get; }
        
        Task<ulong> Send(IPendingMessage message);
        Task<ulong> Update(IPendingMessage message);
        Task<bool> Delete(ulong messageId);

        event Action<IRecievedMessage> Received;
        event Action<IRecievedMessage> Updated;
        event Action<ulong> Deleted;
    }
}
