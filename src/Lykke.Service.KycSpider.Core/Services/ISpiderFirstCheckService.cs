using System.Threading.Tasks;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ISpiderFirstCheckService
    {
        Task PerformFirstCheckAsync(string clientId);
    }
}
