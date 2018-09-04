using System;
using Autofac;
using Autofac.Core;
using AzureStorage;
using AzureStorage.Tables;
using Lykke.Common.Log;
using Lykke.Service.KycSpider.Settings;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.KycSpider.Modules
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder AddService<TService, TInterface>(this ContainerBuilder builder, params Parameter[] parameters)
            where TService : TInterface
        {
            builder
                .RegisterType<TService>()
                .As<TInterface>()
                .WithParameters(parameters)
                .SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddNoSqlTableStorage<T>(this ContainerBuilder builder, IReloadingManager<AzureTableSettings> settings)
            where T : class, ITableEntity, new()
        {
            builder.Register(context =>
            {
                var logFactory = context.Resolve<ILogFactory>();
                var connStringManager = settings.Nested(x => x.ConnectionString);
                var tableName = settings.CurrentValue.TableName;

                return AzureTableStorage<T>.Create(connStringManager, tableName, logFactory);
            });

            return builder;
        }

        public static ContainerBuilder ApplyConfig(this ContainerBuilder builder, Action<ContainerBuilder> action)
        {
            action.Invoke(builder);

            return builder;
        }

        public static ContainerBuilder AddPeriodicalHandler<THandler>(this ContainerBuilder builder, params Parameter[] parameters)
            where THandler: IStartable
        {
            builder
                .RegisterType<THandler>()
                .As<IStartable>()
                .WithParameters(parameters)
                .AutoActivate()
                .SingleInstance();

            return builder;
        }
    }
}
