using JetBrains.Annotations;
using Lykke.Service.KycSpider.Core.Settings;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycSpider.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class KycSpiderSettings
    {
        public DbSettings Db { get; set; }

        public SpiderCheckSettings SpiderCheckSettings { get; set; }
        public EuroSpiderServiceSettings EuroSpiderServiceSettings { get; set; }
        public string KycCqrsRabbitConnString { get; set; }
        public string KycCqrsEnvironment { get; set; }
    }
}
