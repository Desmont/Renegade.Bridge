using System.Collections.Generic;
using Renegade.Bridge.Interface;

namespace Renegade.Bridge.Model
{
    public class GatewayEntry
    {
        public string ClientName { get; init; }
        public IRecievedMessage Message { get; set; }

        public ICollection<KeyValuePair<string, ulong>> MessageIds { get; } =
            new List<KeyValuePair<string, ulong>>();
    }
}
