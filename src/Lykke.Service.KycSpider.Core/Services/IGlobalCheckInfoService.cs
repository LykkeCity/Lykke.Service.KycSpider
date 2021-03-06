﻿using System;
using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface IGlobalCheckInfoService
    {
        Task<IGlobalCheckInfo> AddCheckInfo(IGlobalCheckInfo info);
        Task<DateTime?> GetLatestCheckTimestamp();
    }
}
