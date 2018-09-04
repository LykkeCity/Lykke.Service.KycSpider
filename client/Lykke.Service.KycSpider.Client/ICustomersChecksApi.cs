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
        [Get("/getchecksinfo/{clientId}")]
        Task<CustomerChecksInfo> GetChecksInfoAsync(string clientId);

        [Get("/getdocumentinfo/{clientId}/{documentId}")]
        Task<SpiderDocumentInfo> GetDocumentInfoAsync(string clientId, string documentId);


        [Post("/enablecheck/{clientId}/{type}")]
        Task EnablePepCheckAsync(string clientId, string type);

    }
}
