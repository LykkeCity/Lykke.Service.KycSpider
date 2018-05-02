using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycSpider.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
