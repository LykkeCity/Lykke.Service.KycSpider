using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    public class VerifiableCustomerService : IVerifiableCustomerService
    {
        private readonly IVerifiableCustomerInfoRepository _repository;

        public VerifiableCustomerService(IVerifiableCustomerInfoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IVerifiableCustomerInfo> GetAsync(string clientId)
        {
            return await _repository.GetAsync(clientId);
        }

        public async Task<IVerifiableCustomerInfo> DisablePepCheck(string clientId)
        {
            return await _repository.UpdateCheckStatesAsync(clientId, pep: false);
        }

        public async Task<IVerifiableCustomerInfo> DisableCrimeCheck(string clientId)
        {
            return await _repository.UpdateCheckStatesAsync(clientId, crime: false);
        }

        public async Task<IVerifiableCustomerInfo> DisableSanctionCheck(string clientId)
        {
            return await _repository.UpdateCheckStatesAsync(clientId, sanction: false);
        }
    }
}
