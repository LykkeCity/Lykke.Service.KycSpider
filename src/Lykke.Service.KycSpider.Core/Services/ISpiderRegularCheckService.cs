﻿using System.Threading.Tasks;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ISpiderRegularCheckService
    {
        Task PerformRegularCheckAsync();
    }
}
