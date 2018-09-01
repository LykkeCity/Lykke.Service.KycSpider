using System.ComponentModel.DataAnnotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycSpider.Settings
{
    public class AzureTableSettings
    {
        [AzureTableCheck]
        public string ConnectionString { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string TableName { get; set; }
    }
}
