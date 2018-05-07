using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.KycSpider.Core.Domain.CheckPerson;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    public class CheckPersonResultDiffService : ICheckPersonResultDiffService
    {
        private static readonly IEqualityComparer<IKycCheckPersonProfile> ResultComparer = new KycCheckPersonResultEqualityComparer();
        private static readonly Predicate<string> PepCategoryPredicate = category => category.Contains("pep");
        private static readonly Predicate<string> CrimeCategoryPredicate = category => category.Contains("crime");
        private static readonly Predicate<string> SanctionCategoryPredicate = category => category.Contains("sanction");

        public async Task<IReadOnlyCollection<IGlobalCheckPersonResult>> ComputeAllDiffsAsync(IEnumerable<IGlobalCheckPersonRequest> requests)
        {
            return await Task.Run(() => requests.Select(ComputeAllDiffs).ToArray());
        }

        public ICheckPersonResultDiff ComputeDiffWithEmptyByPep(IKycCheckPersonResult result)
        {
            return ComputeDiffWithEmpty(result, PepCategoryPredicate);
        }

        public ICheckPersonResultDiff ComputeDiffWithEmptyByCrime(IKycCheckPersonResult result)
        {
            return ComputeDiffWithEmpty(result, CrimeCategoryPredicate);
        }

        public ICheckPersonResultDiff ComputeDiffWithEmptyBySanction(IKycCheckPersonResult result)
        {
            return ComputeDiffWithEmpty(result, SanctionCategoryPredicate);
        }

        private static ICheckPersonResultDiff ComputeDiffWithEmpty(IKycCheckPersonResult result, Predicate<string> categoryPredicate)
        {
            var filteredProfiles = FilterByCategory(result.PersonProfiles, categoryPredicate);

            return new CheckPersonResultDiff
            {
                AddedProfiles = filteredProfiles.ToArray(),
                ChangedProfiles = Array.Empty<IChangedCheckPersonProfile>(),
                RemovedProfiles = Array.Empty<IKycCheckPersonProfile>()
            };
        }


        private static IGlobalCheckPersonResult ComputeAllDiffs(IGlobalCheckPersonRequest request)
        {
            var (current, previous) = (request.CurrentResult, request.PreviousResult);

            return new GlobalCheckPersonResult
            {
                Request = request,
                PepDiff = ComputeDiffByCategory(current, previous, PepCategoryPredicate),
                CrimeDiff = ComputeDiffByCategory(current, previous, CrimeCategoryPredicate),
                SanctionDiff = ComputeDiffByCategory(current, previous, SanctionCategoryPredicate)
            };
        }

        private static ICheckPersonResultDiff ComputeDiffByCategory(IKycCheckPersonResult current,
            IKycCheckPersonResult previous, Predicate<string> categoryPredicate)
        {
            var (oldProfiles, newProfiles) = RemoveIntersection(
                FilterByCategory(previous.PersonProfiles, categoryPredicate),
                FilterByCategory(current.PersonProfiles, categoryPredicate), ResultComparer);

            var oldProfileIds = oldProfiles.Select(x => x.SpiderProfileId);
            var newProfileIds = oldProfiles.Select(x => x.SpiderProfileId);

            var changedIds = oldProfileIds
                .Intersect(newProfileIds)
                .ToHashSet();

            var changedProfiles = oldProfiles
                .Join(newProfiles,
                    x => x.SpiderProfileId,
                    y => y.SpiderProfileId,
                    (x, y) => new ChangedCheckPersonProfile
                    {
                        Previous = x,
                        Current = y
                    })
                .Where(p => changedIds.Contains(p.Previous.SpiderProfileId))
                .ToArray();

            var removedProfiles = oldProfiles
                .Where(x => !changedIds.Contains(x.SpiderProfileId))
                .ToArray();

            var addedProfiles = newProfiles
                .Where(x => !changedIds.Contains(x.SpiderProfileId))
                .ToArray();

            return new CheckPersonResultDiff
            {
                ChangedProfiles = changedProfiles,
                RemovedProfiles = removedProfiles,
                AddedProfiles = addedProfiles
            };
        }

        private static IEnumerable<IKycCheckPersonProfile> FilterByCategory(IEnumerable<IKycCheckPersonProfile> profiles, Predicate<string> categoryPredicate)
        {
            return profiles
                .Where(x => x.MatchingLegalCategories.Any(c => categoryPredicate(c)))
                .Select(x => new KycCheckPersonProfile
                {
                    Citizenships = x.Citizenships,
                    DatesOfBirth = x.DatesOfBirth,
                    Residences = x.Residences,
                    Name = x.Name,
                    SpiderProfileId = x.SpiderProfileId,
                    MatchingLegalCategories = x.MatchingLegalCategories.FindAll(categoryPredicate)
                });
        }

        private static (HashSet<T>, HashSet<T>) RemoveIntersection<T>(IEnumerable<T> firstSource, IEnumerable<T> secondSource, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;

            var first = firstSource.ToHashSet(comparer);
            var second = secondSource.ToHashSet(comparer);

            var intersection = first.Intersect(second).ToHashSet(comparer);

            first.RemoveWhere(x => intersection.Contains(x, comparer));
            second.RemoveWhere(x => intersection.Contains(x, comparer));

            return (first, second);
        }

        private class KycCheckPersonResultEqualityComparer : IEqualityComparer<IKycCheckPersonProfile>
        {
            public bool Equals(IKycCheckPersonProfile x, IKycCheckPersonProfile y)
            {
                return x.SpiderProfileId == y.SpiderProfileId
                       && x.Name == y.Name
                       && IsSameStringSets(x.Citizenships, y.Citizenships)
                       && IsSameStringSets(x.DatesOfBirth, y.DatesOfBirth)
                       && IsSameStringSets(x.MatchingLegalCategories, y.MatchingLegalCategories)
                       && IsSameStringSets(x.Residences, y.Residences);
            }

            public int GetHashCode(IKycCheckPersonProfile obj)
            {
                return obj.SpiderProfileId.GetHashCode();
            }

            private static bool IsSameStringSets(IEnumerable<string> first, IEnumerable<string> second)
            {
                return first.ToHashSet().SetEquals(second);
            }
        }
    }
}
