using System.Threading.Tasks;
using Lykke.Service.KycSpider.Client.AutorestClient.Models;

namespace Lykke.Service.KycSpider.Client
{
    /// <summary>
    /// Provides methods to interact with Kyc Spider Service
    /// </summary>
    public interface IKycSpiderClient
    {
        /// <summary>
        /// Gets spider-specific information about check document
        /// </summary>
        Task<SpiderDocumentInfo> GetSpiderDocumentInfoAsync(string clientId, string documentId);

        /// <summary>
        /// Gets information about customer which is checks regularly by Kyc Spider Service
        /// </summary>
        Task<VerifiableCustomerInfo> GetVerifiableCustomerInfoAsync(string clientId);

        /// <summary>
        /// Disables regular pep check of customer
        /// </summary>
        Task DisableCustomerPepCheckAsync(string clientId);

        /// <summary>
        /// Disables regular crime check of customer
        /// </summary>
        Task DisableCustomerCrimeCheckAsync(string clientId);

        /// <summary>
        /// Disables regular sanction check of customer
        /// </summary>
        Task DisableCustomerSanctionCheckAsync(string clientId);
    }
}
