using System.Threading.Tasks;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Projections
{
    public class FirstRunProjection
    {
        private readonly ISpiderFirstCheckService _firstCheckService;

        public FirstRunProjection
        (
            ISpiderFirstCheckService firstCheckService
        )
        {
            _firstCheckService = firstCheckService;
        }
      
        public async Task Handle(ChangeStatusEvent cmd)
        {
            if (cmd.NewStatus == CheckProfilePorcess.ReviewDoneState.Name && cmd.ProfileType == KycProfile.LykkeEurope)
            {
                await _firstCheckService.PerformFirstCheckAsync(cmd.ClientId);
            }
        }
    }
}
