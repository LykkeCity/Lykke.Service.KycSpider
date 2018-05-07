using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
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
            var model = _mapper.Map<GlobalCheckInfo>(info);
            model.Timestamp = DateTimeOffset.UtcNow;

            var entity = await _repository.AddOrUpdateAsync(model);
            _latestCheckTimestamp = entity.Timestamp.DateTime;

            return entity;
        }

        public async Task<DateTime?> GetLatestCheckTimestamp()
        {
            if (_isRequestForLatestCheckPerformed)
            {
                return _latestCheckTimestamp;
            }

            var timestamp = (await _repository.GetAllByYearAsync(DateTime.UtcNow.Year)).FirstOrDefault()?.Timestamp;
            _latestCheckTimestamp = timestamp?.DateTime;
            _isRequestForLatestCheckPerformed = true;

            return _latestCheckTimestamp;
        }
    }
}
