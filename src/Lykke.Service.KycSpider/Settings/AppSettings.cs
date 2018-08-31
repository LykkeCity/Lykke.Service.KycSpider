﻿using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.KycSpider.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public KycSpiderSettings KycSpiderService { get; set; }
    }
}
