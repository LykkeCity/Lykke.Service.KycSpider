using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.KycSpider.Client 
{
    /// <summary>
    /// KycSpider client settings.
    /// </summary>
    public class KycSpiderServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
