using System.Threading.Tasks;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
