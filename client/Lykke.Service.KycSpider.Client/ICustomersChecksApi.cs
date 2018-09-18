using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Refit;

namespace Lykke.Service.KycSpider.Client
{
    /// <summary>
    /// ICustomersChecksApi interface.
    /// </summary>
    [PublicAPI]
    public interface ICustomersChecksApi
    {
        [Get("/api/CustomersChecks/getchecksinfo/{clientId}")]
        Task<CustomerChecksInfo> GetChecksInfoAsync(string clientId);

        [Get("/api/CustomersChecks/getdocumentinfo/{clientId}/{documentId}")]
        Task<SpiderDocumentInfo> GetDocumentInfoAsync(string clientId, string documentId);


        [Post("/api/CustomersChecks/enablecheck/{clientId}/{type}")]
        Task EnableCheckAsync(string clientId, string type);

    }
}
