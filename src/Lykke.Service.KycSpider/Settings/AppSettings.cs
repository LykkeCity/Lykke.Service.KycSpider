using JetBrains.Annotations;
using Lykke.Service.KycSpider.Settings.ServiceSettings;
using Lykke.Service.KycSpider.Settings.SlackNotifications;
using Lykke.Service.PersonalData.Settings;

namespace Lykke.Service.KycSpider.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings
    {
        public KycSpiderSettings KycSpiderService { get; set; }

        public PersonalDataServiceClientSettings PersonalDataServiceClient { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
