using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.KycSpider.Core.Domain.CheckPerson;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ICheckPersonResultDiffService
    {
        Task<IReadOnlyCollection<IGlobalCheckPersonResult>> ComputeAllDiffsAsync(IEnumerable<IGlobalCheckPersonRequest> requests);
        ICheckPersonResultDiff ComputeDiffWithEmptyByPep(IKycCheckPersonResult result);
        ICheckPersonResultDiff ComputeDiffWithEmptyByCrime(IKycCheckPersonResult result);
        ICheckPersonResultDiff ComputeDiffWithEmptyBySanction(IKycCheckPersonResult result);
    }
}
