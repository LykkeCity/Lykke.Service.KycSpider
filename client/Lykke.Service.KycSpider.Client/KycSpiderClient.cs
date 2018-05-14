using System.Threading.Tasks;
using Lykke.Service.KycSpider.Client.AutorestClient;
using Lykke.Service.KycSpider.Client.AutorestClient.Models;

namespace Lykke.Service.KycSpider.Client
{
    /// <summary>
    /// Provides methods to interact with Kyc Spider Service
    /// </summary>
    public class KycSpiderClient : IKycSpiderClient
    {
        private readonly IKycSpiderAPI _api;

        /// <summary>
        /// Initializes a new instance of the KycSpiderClient class.
        /// </summary>
        public KycSpiderClient(IKycSpiderAPI api)
        {
            _api = api;
        }

        /// <summary>
        /// Gets spider-specific information about check document
        /// </summary>
        public Task<SpiderDocumentInfo> GetSpiderDocumentInfoAsync(string clientId, string documentId)
        {
            return _api.ApiSpiderDocumentsByClientIdByDocumentIdGetAsync(clientId, documentId);
        }

        /// <summary>
        /// Gets information about customer which is checks regularly by Kyc Spider Service
        /// </summary>
        public Task<VerifiableCustomerInfo> GetVerifiableCustomerInfoAsync(string clientId)
        {
            return _api.ApiVerifiableCustomersByClientIdGetAsync(clientId);
        }

        /// <summary>
        /// Disables regular pep check of customer
        /// </summary>
        public Task DisableCustomerPepCheckAsync(string clientId)
        {
            return _api.ApiVerifiableCustomersDisablecheckByClientIdPepPostAsync(clientId);
        }

        /// <summary>
        /// Disables regular crime check of customer
        /// </summary>
        public Task DisableCustomerCrimeCheckAsync(string clientId)
        {
            return _api.ApiVerifiableCustomersDisablecheckByClientIdCrimePostAsync(clientId);
        }

        /// <summary>
        /// Disables regular sanction check of customer
        /// </summary>
        public Task DisableCustomerSanctionCheckAsync(string clientId)
        {
            return _api.ApiVerifiableCustomersDisablecheckByClientIdSanctionPostAsync(clientId);
        }
    }
}
