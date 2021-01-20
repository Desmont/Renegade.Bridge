using System.Collections.Generic;
using Renegade.Bridge.Interface;

namespace Renegade.Bridge.Model
{
    public class GatewayEntry
    {
        public string AccountName { get; set; }
        public IRecievedMessage Message { get; set; }

        public ICollection<KeyValuePair<string, ulong>> MessageIds { get; } =
            new List<KeyValuePair<string, ulong>>();
    }
}
