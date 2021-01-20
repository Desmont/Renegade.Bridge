using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Renegade.Bridge.Configuration;
using Renegade.Bridge.Controller;
using Renegade.Bridge.Interface;

namespace Renegade.Bridge.Factory
{
    public class GatewayFactory
    {
        private readonly AccountFactory _accountFactory;
        private readonly BridgeConfiguration _configuration;
        private readonly IDictionary<string, IGatewayController> _gateways;

        public GatewayFactory(BridgeConfiguration configuration, AccountFactory accountFactory)
        {
            _configuration = configuration;
            _accountFactory = accountFactory;
            _gateways = new Dictionary<string, IGatewayController>();
        }

        public IGatewayController GetGateway([NotNull] string name)
        {
            if (_gateways.ContainsKey(name))
            {
                return _gateways[name];
            }

            var gatewayOpts = _configuration.Gateways.Single(x => x.Name == name);
            var gateway = new GatewayController(gatewayOpts.Name, gatewayOpts.Clients.Select(clientOpts =>
            {
                var account = _accountFactory.GetAccount(clientOpts.Account);
                return new ClientController(account, clientOpts);
            }).ToList());

            _gateways.Add(gateway.Name, gateway);
            return gateway;
        }
    }
}
