using System.Collections.Generic;
using System.Linq;
using Renegade.Bridge.Configuration;
using Renegade.Bridge.Factory;
using Renegade.Bridge.Interface;

namespace Renegade.Bridge
{
    public class Bridge
    {
        private readonly BridgeConfiguration _configuration;
        private readonly GatewayFactory _factory;

        private IList<IGatewayController> _gateways;

        public Bridge(BridgeConfiguration configuration, GatewayFactory factory)
        {
            _configuration = configuration;
            _factory = factory;
        }

        public void Initialize()
        {
            _gateways = _configuration.Gateways.Select(x => _factory.GetGateway(x.Name)).ToList();
        }
    }
}
