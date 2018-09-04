using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycSpider.Settings
{
    public class DbSettings
    {
        public AzureTableSettings LogsConnection { get; set; }
        public AzureTableSettings GlobalCheckInfoConnection { get; set; }
        public AzureTableSettings SpiderDocumentInfoConnection { get; set; }
        public AzureTableSettings CustomerChecksInfoConnection { get; set; }
        public AzureTableSettings SpiderCheckResultsConnection { get; set; }
    }
}
