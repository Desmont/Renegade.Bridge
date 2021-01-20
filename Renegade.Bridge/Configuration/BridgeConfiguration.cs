using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Renegade.Bridge.Const;
using Renegade.Bridge.Model.Configuration;

namespace Renegade.Bridge.Configuration
{
    public class BridgeConfiguration
    {
        private const string GatewayPrefix = "gateway.";

        public BridgeConfiguration(IConfiguration config)
        {
            var bridgeTypes = Enum.GetValues(typeof(BridgeType)).Cast<BridgeType>().ToList();

            Accounts = new HashSet<AccountOptions>();
            foreach (var child in config.GetChildren())
            {
                foreach (var accountOpts in bridgeTypes
                    .Where(type => child.Key.StartsWith($"{type}.", StringComparison.OrdinalIgnoreCase))
                    .Select(type => new AccountOptions {Name = child.Key, Type = type}))
                {
                    child.Bind(accountOpts);
                    Accounts.Add(accountOpts);
                }
            }

            Gateways = config.GetChildren()
                .Where(child => child.Key.StartsWith(GatewayPrefix, StringComparison.OrdinalIgnoreCase)).Select(child =>
                {
                    var gatewayOpts = new GatewayOptions {Name = child.Key.Substring(GatewayPrefix.Length)};
                    child.Bind(gatewayOpts);

                    return gatewayOpts;
                }).Where(x => x.Enabled).Select(gatewayOpts =>
                {
                    foreach (var client in gatewayOpts.Clients)
                    {
                        var account = Accounts.SingleOrDefault(x => x.Name == client.Account);

                        if (account == null)
                        {
                            throw new InvalidOperationException($"Client account {client.Account} not found");
                        }
                    }

                    return gatewayOpts;
                }).ToList();
        }

        public IEnumerable<GatewayOptions> Gateways { get; }
        public ISet<AccountOptions> Accounts { get; }
    }
}
