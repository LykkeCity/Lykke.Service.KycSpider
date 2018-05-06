using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface IVerifiableCustomerService
    {
        Task<IVerifiableCustomerInfo> GetAsync(string clientId);
        Task<IVerifiableCustomerInfo> DisablePepCheck(string clientId);
        Task<IVerifiableCustomerInfo> DisableCrimeCheck(string clientId);
        Task<IVerifiableCustomerInfo> DisableSanctionCheck(string clientId);
    }
}
