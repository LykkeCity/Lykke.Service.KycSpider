using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    public class GlobalCheckInfoService : IGlobalCheckInfoService
    {
        private readonly IGlobalCheckInfoRepository _repository;
        private readonly IMapper _mapper;
        private bool _isRequestForLatestCheckPerformed;
        private DateTime? _latestCheckTimestamp;

        public GlobalCheckInfoService
        (
            IGlobalCheckInfoRepository repository,
            IMapper mapper
        )
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IGlobalCheckInfo> AddCheckInfo(IGlobalCheckInfo info)
        {
            var entity = await _repository.AddOrUpdateAsync(info);
            _latestCheckTimestamp = entity.StartDateTime;

            return entity;
        }

        public async Task<DateTime?> GetLatestCheckTimestamp()
        {
            if (_isRequestForLatestCheckPerformed)
            {
                return _latestCheckTimestamp;
            }

            var latestCheck = (await _repository.GetAllByYearAsync(DateTime.UtcNow.Year))
                .OrderByDescending(x => x.StartDateTime).FirstOrDefault();

            _latestCheckTimestamp = latestCheck?.StartDateTime;
            _isRequestForLatestCheckPerformed = true;

            return _latestCheckTimestamp;
        }
    }
}
