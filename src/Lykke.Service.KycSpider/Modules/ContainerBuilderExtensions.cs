using System;
using Autofac;
using Autofac.Core;

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
