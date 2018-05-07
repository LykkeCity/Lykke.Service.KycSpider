using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    public class VerifiableCustomerService : IVerifiableCustomerService
    {
        private readonly IVerifiableCustomerInfoRepository _repository;
        private readonly IMapper _mapper;

        public VerifiableCustomerService(IVerifiableCustomerInfoRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IVerifiableCustomerInfo> GetAsync(string clientId)
        {
            return await _repository.GetAsync(clientId);
        }

        public async Task<IVerifiableCustomerInfo> DisablePepCheck(string clientId)
        {
            var doc = _mapper.Map<VerifiableCustomerInfo>(await _repository.GetAsync(clientId));

            doc.IsPepCheckRequired = false;

            return await _repository.AddOrUpdateAsync(doc);
        }

        public async Task<IVerifiableCustomerInfo> DisableCrimeCheck(string clientId)
        {
            var doc = _mapper.Map<VerifiableCustomerInfo>(await _repository.GetAsync(clientId));

            doc.IsCrimeCheckRequired = false;

            return await _repository.AddOrUpdateAsync(doc);
        }

        public async Task<IVerifiableCustomerInfo> DisableSanctionCheck(string clientId)
        {
            var doc = _mapper.Map<VerifiableCustomerInfo>(await _repository.GetAsync(clientId));

            doc.IsSanctionCheckRequired = false;

            return await _repository.AddOrUpdateAsync(doc);
        }
    }
}
