using System.Collections.Generic;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public class SpiderProfile : ISpiderProfile
    {
        public string ProfileId { get; set; }
        public string FullName { get; set; }
        public IReadOnlyCollection<string> DatesOfBirth { get; set; }
        public IReadOnlyCollection<string> Citizenships { get; set; }
        public IReadOnlyCollection<string> Residences { get; set; }
        public IReadOnlyCollection<string> MatchingLegalCategories { get; set; }
    }
}
