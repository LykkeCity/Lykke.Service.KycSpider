using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PersonalData.Settings;

namespace Lykke.Service.KycSpider.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public KycSpiderSettings KycSpiderService { get; set; }


        public PersonalDataServiceClientSettings PersonalDataServiceClient { get; set; }

        public KycServiceClientSettings KycServiceClient { get; set; }
    }
}
