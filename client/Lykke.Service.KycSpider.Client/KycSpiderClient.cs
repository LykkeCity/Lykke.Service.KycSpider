using Lykke.HttpClientGenerator;

namespace Lykke.Service.KycSpider.Client
{
    /// <summary>
    /// KycSpider API aggregating interface.
    /// </summary>
    public class KycSpiderClient : IKycSpiderClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to KycSpider Api.</summary>
        public IKycSpiderApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public KycSpiderClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IKycSpiderApi>();
        }
    }
}
