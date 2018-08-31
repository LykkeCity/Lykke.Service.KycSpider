using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.PeriodicalHandlers;
using Lykke.Service.KycSpider.Services;
using Lykke.Service.KycSpider.Services.Repositories;
using Lykke.Service.KycSpider.Settings;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

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
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();


            builder
                .AddService<CheckPersonResultDiffService, ICheckPersonResultDiffService>()
                .AddService<GlobalCheckInfoService, IGlobalCheckInfoService>()
                .AddService<SpiderDocumentsService, ISpiderDocumentsService>()
                .AddService<VerifiableCustomerService, IVerifiableCustomerService>()
                .AddService<SpiderRegularCheckService, ISpiderRegularCheckService>()
                .AddService<SpiderInstantCheckService, ISpiderInstantCheckService>()
                .AddService<SpiderCheckService, ISpiderCheckService>(TypedParameter.From(_settings.CurrentValue.EuroSpiderServiceSettings))

                .AddService<SpiderCheckManagerService, ISpiderCheckManagerService>(
                    TypedParameter.From(_settings.CurrentValue.SpiderCheckSettings))


                .AddNoSQLTableStorage<SpiderCheckResultEntity>(_settings.Nested(x => x.Db.SpiderCheckResultsConnection))
                .AddNoSQLTableStorage<GlobalCheckInfoEntity>(_settings.Nested(x => x.Db.GlobalCheckInfoConnection))
                .AddNoSQLTableStorage<SpiderDocumentInfoEntity>(_settings.Nested(x => x.Db.SpiderDocumentInfoConnection))
                .AddNoSQLTableStorage<VerifiableCustomerInfoEntity>(_settings.Nested(x => x.Db.VerifiableCustomerInfoConnection))

                .AddService<GlobalCheckInfoRepository, IGlobalCheckInfoRepository>()
                .AddService<SpiderDocumentInfoRepository, ISpiderDocumentInfoRepository>()
               .AddService<VerifiableCustomerInfoRepository, IVerifiableCustomerInfoRepository>()
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
