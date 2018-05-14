using System;
using Autofac;
using Common.Log;
using Lykke.Service.KycSpider.Client.AutorestClient;

namespace Lykke.Service.KycSpider.Client
{
    /// <summary>
    /// Provides extensions methods to configure Autofac DI-container
    /// </summary>
    public static class AutofacExtension
    {
        private static void RegisterKycSpiderClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterType<KycSpiderClient>()
                .WithParameter("serviceUrl", serviceUrl)
                .WithParameter("api", new KycSpiderAPI())
                .As<IKycSpiderClient>()
                .SingleInstance();
        }


        /// <summary>
        /// Provides extensions methods to join client to Autofac DI-container
        /// </summary>
        public static void RegisterKycSpiderClient(this ContainerBuilder builder, KycSpiderServiceClientSettings settings, ILog log)
        {
            builder.RegisterKycSpiderClient(settings?.ServiceUrl, log);
        }
    }
}
