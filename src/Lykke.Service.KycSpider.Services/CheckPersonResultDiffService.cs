using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Lykke.Service.KycSpider.Core.Domain.PersonDiff;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    public class CheckPersonResultDiffService : ICheckPersonResultDiffService
    {
        private static readonly IEqualityComparer<ISpiderProfile> ResultComparer = new KycCheckPersonResultEqualityComparer();
        private static readonly Func<string, bool> PepCategoryPredicate = category => category.Contains("pep");
        private static readonly Func<string, bool> CrimeCategoryPredicate = category => category.Contains("crime");
        private static readonly Func<string, bool> SanctionCategoryPredicate = category => category.Contains("sanction");

        public IPersonDiffResult ComputeAllDiffs(IPersonDiffRequest request)
        {
            var (current, previous) = (request.CurrentResult, request.PreviousResult);

            return new PersonDiffResult
            {
                PepDiff = ComputeDiffByCategory(current, previous, PepCategoryPredicate),
                CrimeDiff = ComputeDiffByCategory(current, previous, CrimeCategoryPredicate),
                SanctionDiff = ComputeDiffByCategory(current, previous, SanctionCategoryPredicate)
            };
        }

        public ISpiderCheckResultDiff ComputeDiffByPep(ISpiderCheckResult current, ISpiderCheckResult previous)
        {
            return ComputeDiffByCategory(current, previous, PepCategoryPredicate);
        }

        public ISpiderCheckResultDiff ComputeDiffByCrime(ISpiderCheckResult current, ISpiderCheckResult previous)
        {
            return ComputeDiffByCategory(current, previous, CrimeCategoryPredicate);
        }

        public ISpiderCheckResultDiff ComputeDiffBySanction(ISpiderCheckResult current, ISpiderCheckResult previous)
        {
            return ComputeDiffByCategory(current, previous, SanctionCategoryPredicate);
        }

        public ISpiderCheckResultDiff ComputeDiffWithEmptyByPep(ISpiderCheckResult result)
        {
            return ComputeDiffWithEmpty(result, PepCategoryPredicate);
        }

        public ISpiderCheckResultDiff ComputeDiffWithEmptyByCrime(ISpiderCheckResult result)
        {
            return ComputeDiffWithEmpty(result, CrimeCategoryPredicate);
        }

        public ISpiderCheckResultDiff ComputeDiffWithEmptyBySanction(ISpiderCheckResult result)
        {
            return ComputeDiffWithEmpty(result, SanctionCategoryPredicate);
        }

        private static ISpiderCheckResultDiff ComputeDiffWithEmpty(ISpiderCheckResult result, Func<string, bool> categoryPredicate)
        {
            var filteredProfiles = FilterByCategory(result.PersonProfiles, categoryPredicate);

            return new SpiderCheckResultDiff
            {
                AddedProfiles = filteredProfiles.Select(Mapper.Map<SpiderProfile>).ToArray(),
                ChangedProfiles = Array.Empty<SpiderProfilePair>(),
                RemovedProfiles = Array.Empty<SpiderProfile>()
            };
        }

        private static ISpiderCheckResultDiff ComputeDiffByCategory
        (
            ISpiderCheckResult current,
            ISpiderCheckResult previous,
            Func<string, bool> categoryPredicate
        )
        {
            var (oldProfiles, newProfiles) = RemoveIntersection(
                FilterByCategory(previous.PersonProfiles, categoryPredicate),
                FilterByCategory(current.PersonProfiles, categoryPredicate), ResultComparer);

            var oldProfileIds = oldProfiles.Select(x => x.ProfileId);
            var newProfileIds = oldProfiles.Select(x => x.ProfileId);

            var changedIds = oldProfileIds
                .Intersect(newProfileIds)
                .ToHashSet();

            var changedProfiles = oldProfiles
                .Join(newProfiles,
                    x => x.ProfileId,
                    y => y.ProfileId,
                    (x, y) => new SpiderProfilePair
                    {
                        Previous = Mapper.Map<SpiderProfile>(x),
                        Current = Mapper.Map<SpiderProfile>(y)
                    })
                .Where(p => changedIds.Contains(p.Previous.ProfileId))
                .ToArray();

            var removedProfiles = oldProfiles
                .Where(x => !changedIds.Contains(x.ProfileId))
                .Select(Mapper.Map<SpiderProfile>)
                .ToArray();

            var addedProfiles = newProfiles
                .Where(x => !changedIds.Contains(x.ProfileId))
                .Select(Mapper.Map<SpiderProfile>)
                .ToArray();

            return new SpiderCheckResultDiff
            {
                ChangedProfiles = changedProfiles,
                RemovedProfiles = removedProfiles,
                AddedProfiles = addedProfiles
            };
        }

        private static IEnumerable<ISpiderProfile> FilterByCategory(IEnumerable<ISpiderProfile> profiles, Func<string, bool> categoryPredicate)
        {
            return profiles
                .Where(x => x.MatchingLegalCategories.Any(categoryPredicate))
                .Select(x => new SpiderProfile
                {
                    Citizenships = x.Citizenships,
                    DatesOfBirth = x.DatesOfBirth,
                    Residences = x.Residences,
                    FullName = x.FullName,
                    ProfileId = x.ProfileId,
                    MatchingLegalCategories = x.MatchingLegalCategories.Where(categoryPredicate).ToArray()
                });
        }

        private static (HashSet<T>, HashSet<T>) RemoveIntersection<T>(IEnumerable<T> firstSource, IEnumerable<T> secondSource, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;

            var first = firstSource.ToHashSet(comparer);
            var second = secondSource.ToHashSet(comparer);

            var intersection = first.Intersect(second, comparer).ToHashSet(comparer);

            first.RemoveWhere(x => intersection.Contains(x, comparer));
            second.RemoveWhere(x => intersection.Contains(x, comparer));

            return (first, second);
        }

        private class KycCheckPersonResultEqualityComparer : IEqualityComparer<ISpiderProfile>
        {
            public bool Equals(ISpiderProfile x, ISpiderProfile y)
            {
                return x.ProfileId == y.ProfileId
                       && x.FullName == y.FullName
                       && IsSameStringSets(x.Citizenships, y.Citizenships)
                       && IsSameStringSets(x.DatesOfBirth, y.DatesOfBirth)
                       && IsSameStringSets(x.MatchingLegalCategories, y.MatchingLegalCategories)
                       && IsSameStringSets(x.Residences, y.Residences);
            }

            public int GetHashCode(ISpiderProfile obj)
            {
                return obj.ProfileId.GetHashCode();
            }

            private static bool IsSameStringSets(IEnumerable<string> first, IEnumerable<string> second)
            {
                return first.ToHashSet().SetEquals(second);
            }
        }
    }
}
