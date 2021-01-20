using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renegade.Bridge.Configuration;
using Renegade.Bridge.Const;
using Renegade.Bridge.Controller.Account;
using Renegade.Bridge.Interface;

namespace Renegade.Bridge.Factory
{
    public class AccountFactory
    {
        private readonly IDictionary<string, IAccountController> _accounts;
        private readonly BridgeConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public AccountFactory(BridgeConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            _accounts = new Dictionary<string, IAccountController>();
        }

        public IAccountController GetAccount([NotNull] string name)
        {
            if (_accounts.ContainsKey(name))
            {
                return _accounts[name];
            }

            var accountOpts = _configuration.Accounts.Single(x => x.Name == name);

            IAccountController account;
            switch (accountOpts.Type)
            {
                case BridgeType.Discord:
                    account = new DiscordController(accountOpts, _serviceProvider.GetRequiredService<ILoggerFactory>());
                    break;
                case BridgeType.Slack:
                    // TODO MKA add slack support
                    return null;
                // account = new SlackController {Name = accountOpts.Name};
                // break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _accounts.Add(account.Name, account);

            return account;
        }
    }
}
