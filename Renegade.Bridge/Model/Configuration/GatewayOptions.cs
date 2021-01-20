using System.Collections.Generic;

namespace Renegade.Bridge.Model.Configuration
{
    public class GatewayOptions
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public IEnumerable<ClientOptions> Clients { get; set; }
    }
}
