using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.KycSpider.Settings
{
    public class AzureTableSettings
    {
        [Required(AllowEmptyStrings = false)]
        public string ConnectionString { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string TableName { get; set; }
    }
}
