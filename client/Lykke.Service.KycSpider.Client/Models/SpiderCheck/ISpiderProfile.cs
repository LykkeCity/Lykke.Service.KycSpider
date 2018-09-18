using System.Collections.Generic;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public interface ISpiderProfile
    {
        string ProfileId { get; }

        string FullName { get; }

        IReadOnlyCollection<string> DatesOfBirth { get; }

        IReadOnlyCollection<string> Citizenships { get; }

        IReadOnlyCollection<string> Residences { get; }

        IReadOnlyCollection<string> MatchingLegalCategories { get; }
    }
}
