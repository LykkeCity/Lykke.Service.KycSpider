using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Refit;

namespace Lykke.Service.KycSpider.Client
{
    /// <summary>
    /// ISpiderManageApi interface.
    /// </summary>
    [PublicAPI]
    public interface ISpiderManageApi
    {
        [Post("/api/SpiderManage/startregulalcheck")]
        Task StartRegularCheckAsync();

        [Post("/api/SpiderManage/dofirstcheck/{clientId}")]
        Task DoFirstCheckAsync(string clientId);

        [Post("/api/SpiderManage/movefirstcheck/{clientId}")]
        Task<SpiderDocumentAutoStatusGroup> MoveFirstCheckAsync(string clientId, ISpiderCheckResult spiderResult);
    }
}
