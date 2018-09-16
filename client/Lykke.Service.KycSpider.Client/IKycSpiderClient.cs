using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Client
{
    /// <summary> KycSpider client aggregating interface. </summary>
    [PublicAPI]
    public interface IKycSpiderClient
    {
        /// <summary> CustomersChecksApi interface </summary>
        ICustomersChecksApi CustomersChecksApi { get; }

        ISpiderManageApi SpiderManageApi { get; }

        Task<CustomerChecksInfo> GetChecksInfoAsync(string clientId);

        Task<SpiderDocumentInfo> GetDocumentInfoAsync(string clientId, string documentId);

        Task EnableCheckAsync(string clientId, string type);

        Task StartRegularCheckAsync();

        Task DoFirstCheckAsync(string clientId);

        Task<SpiderDocumentAutoStatusGroup> MoveFirstCheckAsync(string clientId, ISpiderCheckResult spiderResult);
    }
}
