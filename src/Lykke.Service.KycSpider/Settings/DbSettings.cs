using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycSpider.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }

        public AzureTableSettings GlobalCheckInfoConnection { get; set; }
        public AzureTableSettings SpiderDocumentInfoConnection { get; set; }
        public AzureTableSettings VerifiableCustomerInfoConnection { get; set; }
        public AzureTableSettings SpiderCheckResultsConnection { get; set; }
    }
}
