using System;
using System.Globalization;
using AutoMapper;
using Lykke.AzureStorage.Tables;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class GlobalCheckInfoEntity : AzureTableEntity, IGlobalCheckInfo
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public int TotalClients { get; set; }
        public int SpiderChecks { get; set; }
        public int PepSuspects { get; set; }
        public int CrimeSuspects { get; set; }
        public int SanctionSuspects { get; set; }
        public int TotalProfiles { get; set; }
        public int AddedProfiles { get; set; }
        public int RemovedProfiles { get; set; }
        public int ChangedProfiles { get; set; }

        public static string GeneratePartitionKey(DateTime startDateTime) => GeneratePartitionKey(startDateTime.Year);
        public static string GeneratePartitionKey(int year) => year.ToString(CultureInfo.InvariantCulture);
        public static string GenerateRowKey(DateTime startDateTime) => startDateTime.ToString(CultureInfo.InvariantCulture);

        public static GlobalCheckInfoEntity Create(IGlobalCheckInfo src)
        {
            var entity = Mapper.Map<GlobalCheckInfoEntity>(src);

            entity.PartitionKey = GeneratePartitionKey(src.StartDateTime);
            entity.RowKey = GenerateRowKey(src.StartDateTime);

            return entity;
        }
    }
}
