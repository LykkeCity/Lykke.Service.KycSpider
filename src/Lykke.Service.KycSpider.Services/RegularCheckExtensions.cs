using System;
using System.Threading.Tasks;
using Lykke.Service.Kyc.Abstractions.Domain.KycDocuments.Data;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    internal static class RegularCheckExtensions
    {
        public static int ToOneOrZero(this bool flag)
        {
            return flag ? 1 : 0;
        }

        public static bool GetIsCheckRequired(this ICustomerChecksInfo customer, string apiType)
        {
            switch (apiType)
            {
                case PepSpiderCheck.ApiType:
                    return customer.IsPepCheckRequired;
                case CrimeSpiderCheck.ApiType:
                    return customer.IsCrimeCheckRequired;
                case SanctionSpiderCheck.ApiType:
                    return customer.IsSanctionCheckRequired;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static string GetLatestCheckId(this ICustomerChecksInfo customer, string apiType)
        {
            switch (apiType)
            {
                case PepSpiderCheck.ApiType:
                    return customer.LatestPepCheckId;
                case CrimeSpiderCheck.ApiType:
                    return customer.LatestCrimeCheckId;
                case SanctionSpiderCheck.ApiType:
                    return customer.LatestSanctionCheckId;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static ISpiderCheckResultDiff ComputeDiff(this ICheckPersonResultDiffService service, string apiType, ISpiderCheckResult current, ISpiderCheckResult previous)
        {
            switch (apiType)
            {
                case PepSpiderCheck.ApiType:
                    return service.ComputeDiffByPep(current, previous);
                case CrimeSpiderCheck.ApiType:
                    return service.ComputeDiffByCrime(current, previous);
                case SanctionSpiderCheck.ApiType:
                    return service.ComputeDiffBySanction(current, previous);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Task<ICustomerChecksInfo> UpdateCheckStatesAsync(this ICustomerChecksInfoRepository repository, string clientId, string apiType, bool state)
        {
            switch (apiType)
            {
                case PepSpiderCheck.ApiType:
                    return repository.UpdateCheckStatesAsync(clientId, pep: state);
                case CrimeSpiderCheck.ApiType:
                    return repository.UpdateCheckStatesAsync(clientId, crime: state);
                case SanctionSpiderCheck.ApiType:
                    return repository.UpdateCheckStatesAsync(clientId, sanction: state);
                default:
                    throw new InvalidOperationException();
            }
        }

        public static Task<ICustomerChecksInfo> UpdateLatestCheckIdAsync(this ICustomerChecksInfoRepository repository, string clientId, string apiType, string checkId)
        {
            switch (apiType)
            {
                case PepSpiderCheck.ApiType:
                    return repository.UpdateCheckIdsAsync(clientId, pepCheckId: checkId);
                case CrimeSpiderCheck.ApiType:
                    return repository.UpdateCheckIdsAsync(clientId, crimeCheckId: checkId);
                case SanctionSpiderCheck.ApiType:
                    return repository.UpdateCheckIdsAsync(clientId, sanctionCheckId: checkId);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
