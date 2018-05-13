using Autofac;
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
            builder
                .AddService<TypedDocumentsService, ITypedDocumentsService>(
                    TypedParameter.From(_settings.CurrentValue.PersonalDataServiceClient))

                .AddService<PersonalDataService, IPersonalDataService>(
                    TypedParameter.From(_settings.CurrentValue.PersonalDataServiceClient));

        }
    }
}
