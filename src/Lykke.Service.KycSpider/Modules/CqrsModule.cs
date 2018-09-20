using System.Collections.Generic;
using Autofac;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.Kyc.Abstractions.Domain.Profile;
using Lykke.Service.KycSpider.Projections;
using Lykke.Service.KycSpider.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.KycSpider.Modules
{
    public class CqrsModule : Module
    {
        private readonly AppSettings _settings;
		private ILogFactory _log;

		public CqrsModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue;                  
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

			var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.KycSpiderService.KycCqrsRabbitConnString };

			builder.Register(ctx => new MessagingEngine(ctx.Resolve<ILogFactory>(),
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
					    "RabbitMq",
					    new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
					                      rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory(ctx.Resolve<ILogFactory>()))).As<IMessagingEngine>();

            builder.RegisterType<FirstRunProjection>();

            builder.Register(ctx =>
            {
                var projection = ctx.Resolve<FirstRunProjection>();

                return new CqrsEngine(
                    ctx.Resolve<ILogFactory>(),
                    ctx.Resolve<IDependencyResolver>(),
                    ctx.Resolve<IMessagingEngine>(),
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver("RabbitMq",
                        SerializationFormat.ProtoBuf, environment: _settings.KycSpiderService.KycCqrsEnvironment)),

                    Register.BoundedContext("kyc-spider")
                        .ListeningEvents(typeof(ChangeStatusEvent))
                        .From("kyc").On("events")
                        .WithProjection(projection, "kyc")
                );
            }).As<ICqrsEngine>().SingleInstance().AutoActivate();
        }
    }
}
