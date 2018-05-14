using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.PeriodicalHandlers;
using Lykke.Service.KycSpider.Settings.ServiceSettings;
using Lykke.Service.KycSpider.Services;
using Lykke.Service.KycSpider.Services.Repositories;
using Lykke.Service.KycSpider.Settings;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.KycSpider.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<KycSpiderSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<KycSpiderSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

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
                .AddService<SpiderCheckService, ISpiderCheckService>(TypedParameter.From(_settings.CurrentValue.EuroSpiderServiceSettings))

                .AddService<SpiderTimerCheckService, ISpiderTimerCheckService>(
                    TypedParameter.From(_settings.CurrentValue.SpiderCheckSettings))

                .AddService<GlobalCheckInfoRepository, IGlobalCheckInfoRepository>(
                    TypedParameter.From(CreateAzureTableStorage<GlobalCheckInfoEntity>(x => x.GlobalCheckInfoConnection)))

                .AddService<SpiderDocumentInfoRepository, ISpiderDocumentInfoRepository>(
                    TypedParameter.From(CreateAzureTableStorage<SpiderDocumentInfoEntity>(x => x.SpiderDocumentInfoConnection)))

                .AddService<VerifiableCustomerInfoRepository, IVerifiableCustomerInfoRepository>(
                    TypedParameter.From(CreateAzureTableStorage<VerifiableCustomerInfoEntity>(x => x.VerifiableCustomerInfoConnection)))

                .AddService<SpiderCheckResultRepository, ISpiderCheckResultRepository>(
                    TypedParameter.From(CreateAzureTableStorage<SpiderCheckResultEntity>(x => x.SpiderCheckResultsConnection)))

                .ApplyConfig(RegisterPeriodicalHandlers);

            builder.Populate(_services);
        }

        private INoSQLTableStorage<T> CreateAzureTableStorage<T>(Func<KycSpiderSettings, AzureTableSettings> expr) where T : class, ITableEntity, new()
        {
            var settings = _settings.Nested(expr);
            
            return AzureTableStorage<T>.Create(settings.Nested(x => x.ConnectionString),
                settings.CurrentValue.TableName, _log);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder
                .AddPeriodicalHandler<SpiderCheckPeriodicalHandler>(
                    TypedParameter.From(_settings.CurrentValue.SpiderCheckSettings));
        }
    }
}
