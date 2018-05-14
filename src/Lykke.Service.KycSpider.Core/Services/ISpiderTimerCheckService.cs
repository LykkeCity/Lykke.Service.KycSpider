using System.Threading.Tasks;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ISpiderTimerCheckService
    {
        Task PerformCheckAsync();
    }
}
