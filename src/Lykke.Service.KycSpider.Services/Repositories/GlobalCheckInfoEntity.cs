using System;
using System.Globalization;
using AutoMapper;
using Lykke.AzureStorage.Tables;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class GlobalCheckInfoEntity : AzureTableEntity, IGlobalCheckInfo
    {
        public int SpiderChecks { get; set; }
        public int PepSuspects { get; set; }
        public int CrimeSuspects { get; set; }
        public int SanctionSuspects { get; set; }
        public int TotalProfiles { get; set; }
        public int AddedProfiles { get; set; }
        public int RemovedProfiles { get; set; }
        public int ChangedProfiles { get; set; }

        public static string GeneratePartitionKey(DateTimeOffset dateTime) => dateTime.Year.ToString(CultureInfo.InvariantCulture);
        public static string GeneratePartitionKey(int year) => year.ToString(CultureInfo.InvariantCulture);
        public static string GenerateRowKey(DateTimeOffset dateTime) => dateTime.ToString(CultureInfo.InvariantCulture);

        public static GlobalCheckInfoEntity Create(IGlobalCheckInfo src)
        {
            var entity = Mapper.Map<GlobalCheckInfoEntity>(src);

            entity.PartitionKey = GeneratePartitionKey(src.Timestamp);
            entity.RowKey = GenerateRowKey(src.Timestamp);

            return entity;
        }
    }
}
