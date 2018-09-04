using System.Threading.Tasks;
using Lykke.HttpClientGenerator;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Client
{
    /// <inheritdoc/>
    public class KycSpiderClient : IKycSpiderClient
    {
        /// <inheritdoc/>
        public ICustomersChecksApi CustomersChecksApi { get; private set; }

        /// <summary>C-tor</summary>
        public KycSpiderClient(IHttpClientGenerator httpClientGenerator)
        {
            CustomersChecksApi = httpClientGenerator.Generate<ICustomersChecksApi>();
        }

        public Task<CustomerChecksInfo> GetChecksInfoAsync(string clientId)
        {
            return CustomersChecksApi.GetChecksInfoAsync(clientId);
        }

        public Task<SpiderDocumentInfo> GetDocumentInfoAsync(string clientId, string documentId)
        {
            return CustomersChecksApi.GetDocumentInfoAsync(clientId, documentId);
        }

        public Task EnablePepCheckAsync(string clientId, string type)
        {
            return CustomersChecksApi.EnablePepCheckAsync(clientId, type);
        }
    }
}
