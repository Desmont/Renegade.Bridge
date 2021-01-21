using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Renegade.Bridge.Configuration;
using Renegade.Bridge.Factory;

namespace Renegade.Bridge
{
    internal class Program
    {
        private IConfiguration _config;
        private IServiceProvider _serviceProvider;

        private static async Task Main(string[] args)
        {
            await new Program().MainAsync();
        }

        public async Task MainAsync()
        {
            _config = BuildConfig();
            _serviceProvider = ConfigureServices();

            var logger = _serviceProvider.GetService<ILogger>();

            try
            {
                var bridge = _serviceProvider.GetRequiredService<Bridge>();
                bridge.Initialize();
            }
            catch (Exception e)
            {
                Debugger.Break();
                logger.Log(LogLevel.Critical, e, e.Message);
                throw;
            }

            await Task.Delay(-1);
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton<Bridge>()
                .AddSingleton<BridgeConfiguration>()
                .AddSingleton<GatewayFactory>()
                .AddSingleton<AccountFactory>()
                // Logging
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(_config.GetSection("Logging"));

                    builder.AddConsole();
                })
                // Extra
                .AddSingleton(_config)
                // Add additional services here...
                .BuildServiceProvider();
        }
    }
}
