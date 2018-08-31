using JetBrains.Annotations;
using Lykke.Service.KycSpider.Core.Settings;

namespace Lykke.Service.KycSpider.Settings.ServiceSettings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class KycSpiderSettings
    {
        public DbSettings Db { get; set; }
        public AzureTableSettings GlobalCheckInfoConnection { get; set; }
        public AzureTableSettings SpiderDocumentInfoConnection { get; set; }
        public AzureTableSettings VerifiableCustomerInfoConnection { get; set; }
        public AzureTableSettings SpiderCheckResultsConnection { get; set; }
        public SpiderCheckSettings SpiderCheckSettings { get; set; }
        public SpiderServiceSettings EuroSpiderServiceSettings { get; set; }
    }
}
