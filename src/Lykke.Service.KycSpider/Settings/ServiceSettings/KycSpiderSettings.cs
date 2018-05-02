using JetBrains.Annotations;

namespace Lykke.Service.KycSpider.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class KycSpiderSettings
    {
        public DbSettings Db { get; set; }
    }
}
