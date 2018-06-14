using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;
using Lykke.Service.KycSpider.Services.EuroSpiderClient;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderCheckService : ISpiderCheckService
    {
        private readonly ISpiderCheckResultRepository _repository;
        private readonly IPersonalDataService _personalDataService;
        private readonly SpiderServiceSettings _settings;
        private readonly ChannelFactory<accessChannel> _euroSpiderChannelFactory;

        public SpiderCheckService
        (
            ISpiderCheckResultRepository repository,
            IPersonalDataService personalDataService,
            SpiderServiceSettings settings
        )
        {
            _repository = repository;
            _personalDataService = personalDataService;
            _settings = settings;
            _euroSpiderChannelFactory = CreateChannelFactory();
        }

        public async Task<ISpiderCheckResult> CheckAsync(string clientId)
        {
            var personalData = await _personalDataService.GetAsync(clientId);

            if (personalData == null)
            {
                throw new InvalidOperationException($"No personal data for ClientId:{clientId} but spider check requested");
            }

            var request = new checkPerson(FormRequest(personalData));
            var result = await PerformCheck(request);
            var mappedResult = MapResult(personalData, result);       

            return await _repository.AddAsync(mappedResult);
        }

        private static PersonCheckData FormRequest(IPersonalData personalData)
        {
            return new PersonCheckData
            {
                residences = Array.Empty<string>(),
                citizenships = new[] { personalData.CountryFromID },
                datesOfBirth = new[]
                {
                    new IncompleteDate
                    {
                        year = personalData.DateOfBirth?.Year ?? -1,
                        month = personalData.DateOfBirth?.Month ?? -1,
                        day = personalData.DateOfBirth?.Day ?? -1
                    }
                },
                customerId = personalData.Id,
                names = new[]
                {
                    new PersonName
                    {
                        firstName = personalData.FirstName,
                        lastName = personalData.LastName
                    }
                }
            };
        }

        private async Task<checkPersonResponse> PerformCheck(checkPerson request)
        {
            using (var channel = _euroSpiderChannelFactory.CreateChannel())
            {
                return await channel.checkPersonAsync(request);
            }
        }

        private ChannelFactory<accessChannel> CreateChannelFactory()
        {
            var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 1024 * 512
            };
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            var epa = new EndpointAddress(_settings.EndpointUrl);
            var factory = new ChannelFactory<accessChannel>(binding, epa);
            factory.Credentials.UserName.UserName = _settings.User;
            factory.Credentials.UserName.Password = _settings.Password;

            return factory;
        }

        private static ISpiderCheckResult MapResult(IPersonalData personalData, checkPersonResponse response)
        {
            var profiles = response.@return.personProfiles ?? Array.Empty<PersonProfile>();

            return new SpiderCheckResult
            {
                CustomerId = personalData.Id,
                VerificationId = response.@return.verificationId,
                CheckDateTime = DateTime.UtcNow,
                PersonProfiles = profiles.Select(MapProfile).ToArray()
            };
        }

        private static SpiderProfile MapProfile(PersonProfile profile)
        {
            return new SpiderProfile
            {
                FullName = profile.name,
                Citizenships = profile.citizenships.ToArray(),
                Residences = profile.residences.ToArray(),
                MatchingLegalCategories = profile.matchingLegalCategories.Select(x => x.ToLower()).ToArray(),
                ProfileId = profile.id,
                DatesOfBirth = profile.datesOfBirth.Select(MapIncompleteDate).ToArray()
            };
        }

        private static string MapIncompleteDate(IncompleteDate date)
        {
            if (date.day > 0)
            {
                return $"{date.year:00}{date.month:00}{date.day:00}";
            }

            if (date.month > 0)
            {
                return $"{date.year:00}{date.month:00}";
            }

            return $"{date.year:00}";
        }
    }
}
