using Renegade.Bridge.Const;

namespace Renegade.Bridge.Model.Configuration
{
    public class AccountOptions
    {
        public BridgeType Type { get; set; }

        public string Name { get; set; }

        public string Server { get; set; }

        public string Token { get; set; }

        // public string WebhookUrl { get; set; }
    }
}
