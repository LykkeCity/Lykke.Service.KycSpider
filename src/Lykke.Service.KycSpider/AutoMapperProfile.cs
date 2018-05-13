using AutoMapper;
using Lykke.AzureStorage.Tables;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Services.Repositories;

namespace Lykke.Service.KycSpider
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMapForEntity<IGlobalCheckInfo, GlobalCheckInfoEntity>();
            CreateMapForEntity<ISpiderDocumentInfo, SpiderDocumentInfoEntity>();
            CreateMapForEntity<IVerifiableCustomerInfo, VerifiableCustomerInfoEntity>();
            CreateMapForEntity<ISpiderCheckResult, SpiderCheckResultEntity>();
        }

        private void CreateMapForEntity<TInterface, TEntity>()
            where TEntity: AzureTableEntity, TInterface
        {
            CreateMap<TInterface, TEntity>(MemberList.Source)
                .IgnoreAllPropertiesWithAnInaccessibleSetter();
        }
    }
}
