using System.Threading.Tasks;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ISpiderCheckManagerService
    {
        Task OnTimerAsync();
        Task<bool> TryStartRegularCheckManuallyAsync();
    }
}
