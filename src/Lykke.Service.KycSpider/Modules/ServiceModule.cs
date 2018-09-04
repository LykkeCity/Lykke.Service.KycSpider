using Autofac;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.PeriodicalHandlers;
using Lykke.Service.KycSpider.Services;
using Lykke.Service.KycSpider.Services.Repositories;
using Lykke.Service.KycSpider.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.KycSpider.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<KycSpiderSettings> _settings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _settings = appSettings.Nested(x => x.KycSpiderService);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .AddService<CheckPersonResultDiffService, ICheckPersonResultDiffService>()
                .AddService<GlobalCheckInfoService, IGlobalCheckInfoService>()
                .AddService<SpiderDocumentsService, ISpiderDocumentsService>()
                .AddService<CustomerChecksService, ICustomerChecksService>()
                .AddService<SpiderRegularCheckService, ISpiderRegularCheckService>()
                .AddService<SpiderFirstCheckService, ISpiderFirstCheckService>()

                .AddService<SpiderCheckService, ISpiderCheckService>(
                    TypedParameter.From(_settings.CurrentValue.EuroSpiderServiceSettings))

                .AddService<SpiderCheckManagerService, ISpiderCheckManagerService>(
                    TypedParameter.From(_settings.CurrentValue.SpiderCheckSettings))


                .AddNoSqlTableStorage<SpiderCheckResultEntity>(_settings.Nested(x => x.Db.SpiderCheckResultsConnection))
                .AddNoSqlTableStorage<GlobalCheckInfoEntity>(_settings.Nested(x => x.Db.GlobalCheckInfoConnection))
                .AddNoSqlTableStorage<SpiderDocumentInfoEntity>(_settings.Nested(x => x.Db.SpiderDocumentInfoConnection))
                .AddNoSqlTableStorage<CustomerChecksInfoEntity>(_settings.Nested(x => x.Db.CustomerChecksInfoConnection))

                .AddService<GlobalCheckInfoRepository, IGlobalCheckInfoRepository>()
                .AddService<SpiderDocumentInfoRepository, ISpiderDocumentInfoRepository>()
                .AddService<CustomerChecksInfoRepository, ICustomerChecksInfoRepository>()
                .AddService<SpiderCheckResultRepository, ISpiderCheckResultRepository>()

                .ApplyConfig(RegisterPeriodicalHandlers);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder
                .AddPeriodicalHandler<SpiderCheckPeriodicalHandler>(
                    TypedParameter.From(_settings.CurrentValue.SpiderCheckSettings));
        }
    }
}
