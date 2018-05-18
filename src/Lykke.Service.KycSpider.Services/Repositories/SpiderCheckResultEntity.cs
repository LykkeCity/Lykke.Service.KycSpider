using System;
using System.Collections.Generic;
using AutoMapper;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class SpiderCheckResultEntity : AzureTableEntity, ISpiderCheckResult
    {
        public string CustomerId => PartitionKey;
        public string ResultId => RowKey;
        public long VerificationId { get; set; }
        public DateTime CheckDateTime { get; set; }

        [JsonValueSerializer]
        public IReadOnlyCollection<SpiderProfile> PersonProfiles { get; set; }

        IReadOnlyCollection<ISpiderProfile> ISpiderCheckResult.PersonProfiles => PersonProfiles;

        public static string GeneratePartitionKey(string clientId) => clientId;
        public static string GenerateRowKey(string resultId) => resultId ?? Guid.NewGuid().ToString();

        public static SpiderCheckResultEntity Create(ISpiderCheckResult src)
        {
            var entity = Mapper.Map<SpiderCheckResultEntity>(src);
            entity.PersonProfiles = Mapper.Map<SpiderProfile[]>(src.PersonProfiles);
            entity.PartitionKey = GeneratePartitionKey(src.CustomerId);
            entity.RowKey = GenerateRowKey(src.ResultId);

            return entity;
        }


    }
}
