using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycSpider.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
