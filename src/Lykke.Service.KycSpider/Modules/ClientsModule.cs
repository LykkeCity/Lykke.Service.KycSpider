using Autofac;
using Lykke.Common.Log;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.Kyc.Client;
using Lykke.Service.KycSpider.Settings;
using Lykke.Service.PersonalData.Client;
using Lykke.Service.PersonalData.Contract;
using Lykke.SettingsReader;

namespace Lykke.Service.KycSpider.Modules
{
    public class ClientsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ClientsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register<IPersonalDataService>(context => new PersonalDataService(
                    _settings.CurrentValue.PersonalDataServiceClient,
                    context.Resolve<ILogFactory>().CreateLog(nameof(PersonalDataService))));

            builder.Register<IRequestableDocumentsService>(context => new RequestableDocumentsServiceClient(
                _settings.CurrentValue.KycServiceClient,
                context.Resolve<ILogFactory>().CreateLog(nameof(RequestableDocumentsServiceClient))));

            builder.Register<IDocumentsQueueReaderService>(context => new DocumentsQueueReaderServiceClient(
                _settings.CurrentValue.KycServiceClient,
                context.Resolve<ILogFactory>().CreateLog(nameof(DocumentsQueueReaderServiceClient))));
        }
    }
}
