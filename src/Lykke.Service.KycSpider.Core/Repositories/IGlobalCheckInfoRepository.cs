using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Repositories
{
    public interface IGlobalCheckInfoRepository
    {
        Task<IGlobalCheckInfo> AddOrUpdateAsync(IGlobalCheckInfo entity);
        Task<IGlobalCheckInfo> GetAsync(DateTime timestamp);
        Task<IEnumerable<IGlobalCheckInfo>> GetAllByYearAsync(int year);
        Task<IGlobalCheckInfo> DeleteAsync(DateTime timestamp);
    }
}
