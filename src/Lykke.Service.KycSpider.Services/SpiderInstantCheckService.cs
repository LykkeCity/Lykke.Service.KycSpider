using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.Kyc.Abstractions.Requests;
using Lykke.Service.Kyc.Client;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Client.Models.Documents;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models.Documents;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderInstantCheckService : ISpiderInstantCheckService
    {
        private readonly ICheckPersonResultDiffService _diffService;
        private readonly ISpiderDocumentInfoRepository _spiderDocumentInfoRepository;
        private readonly IVerifiableCustomerInfoRepository _verifiableCustomerRepository;
        private readonly ITypedDocumentsService _typedDocumentsService;
        private readonly ISpiderCheckService _spiderCheckService;
        private readonly ISpiderCheckResultRepository _spiderCheckResultRepository;
        private readonly IRequestableDocumentsServiceClient _requestableDocumentsService;
        private readonly ILog _log;

        private const string NoSuspectedProfiles = "No suspected profiles";
        private static readonly Changer SpiderChanger = new Changer
        {
            Name = "Spider"
        };

        public SpiderInstantCheckService
        (
            ICheckPersonResultDiffService diffService,
            ISpiderDocumentInfoRepository spiderDocumentInfoRepository,
            IVerifiableCustomerInfoRepository verifiableCustomerRepository,
            ITypedDocumentsService typedDocumentsService,
            ISpiderCheckService spiderCheckService,
            ISpiderCheckResultRepository spiderCheckResultRepository,
            IRequestableDocumentsServiceClient requestableDocumentsService,
            ILog log
        )
        {
            _diffService = diffService;
            _spiderDocumentInfoRepository = spiderDocumentInfoRepository;
            _verifiableCustomerRepository = verifiableCustomerRepository;
            _typedDocumentsService = typedDocumentsService;
            _spiderCheckService = spiderCheckService;
            _spiderCheckResultRepository = spiderCheckResultRepository;
            _requestableDocumentsService = requestableDocumentsService;
            _log = log;
        }

        public async Task PerformInstantCheckAsync()
        {
            await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(PerformInstantCheckAsync), "started");

            var pepDocs = await _typedDocumentsService.GetAllPepCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);
            var crimeDocs = await _typedDocumentsService.GetAllCrimeCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);
            var sanctionDocs = await _typedDocumentsService.GetAllSanctionCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);

            var clientDict = pepDocs
                .Select(x => x.CustomerId)
                .Concat(crimeDocs.Select(x => x.CustomerId))
                .Concat(sanctionDocs.Select(x => x.CustomerId))
                .Distinct()
                .ToDictionary(x => x, x => new InstantCheckState());

            foreach (var pepDoc in pepDocs)
            {
                clientDict[pepDoc.CustomerId].Pep = pepDoc;
            }

            foreach (var crimeDoc in crimeDocs)
            {
                clientDict[crimeDoc.CustomerId].Crime = crimeDoc;
            }

            foreach (var sanctionDoc in sanctionDocs)
            {
                clientDict[sanctionDoc.CustomerId].Sanction = sanctionDoc;
            }

            foreach (var clientId in clientDict.Keys)
            {
                var state = clientDict[clientId];

                var spiderCheckId = (await _verifiableCustomerRepository.GetAsync(clientId))?.LatestSpiderCheckId;

                if (spiderCheckId == null)
                {
                    state.CheckResult = await _spiderCheckService.CheckAsync(clientId);
                    state.IsNewCustomer = true;
                }
                else
                {
                    state.CheckResult = await _spiderCheckResultRepository.GetAsync(clientId, spiderCheckId);
                    state.IsNewCustomer = false;
                }

                await UpdateDocuments(state);
                await UpdateVerifiableCustomer(state);
            }

            await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(PerformInstantCheckAsync), "done");
        }

        private async Task UpdateVerifiableCustomer(InstantCheckState state)
        {
            var clientId = state.CheckResult.CustomerId;
            var latestCheckId = state.CheckResult.ResultId;
            var isPepCheckRequired = IsNotNull(state.Pep);
            var isCrimeCheckRequired = IsNotNull(state.Crime);
            var isSanctionCheckRequired = IsNotNull(state.Sanction);

            if (state.IsNewCustomer)
            {
                await _verifiableCustomerRepository.AddAsync(new VerifiableCustomerInfo
                {
                    CustomerId = clientId,
                    IsPepCheckRequired = isPepCheckRequired,
                    IsCrimeCheckRequired = isCrimeCheckRequired,
                    IsSanctionCheckRequired = isSanctionCheckRequired,
                    LatestSpiderCheckId = latestCheckId
                });
            }
            else
            {
                await _verifiableCustomerRepository.UpdateCheckStatesAsync
                (
                    clientId,
                    pep: ToNullIfFalse(isPepCheckRequired),
                    crime: ToNullIfFalse(isCrimeCheckRequired),
                    sanction: ToNullIfFalse(isSanctionCheckRequired)
                );
            }
        }

        private static bool IsNotNull(object reference)
        {
            return reference != null;
        }

        private static bool? ToNullIfFalse(bool value)
        {
            return value ? true : null as bool?;
        }

        private async Task UpdateDocuments(InstantCheckState state)
        {
            var currentResultId = state.CheckResult.ResultId;

            if (state.Pep != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyByPep(state.CheckResult);
                var doc = state.Pep;

                await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(UpdateDocuments),
                    $"Set status {doc.State} for DocumentId: {doc.DocumentId} for ClientId: {doc.CustomerId}");

                if (IsSuspectedDiff(diff))
                {
                    await _requestableDocumentsService.SubmitDocumentAsync(doc.CustomerId, doc.DocumentId, SpiderChanger);
                }
                else
                {
                    await _requestableDocumentsService.ApproveDocumentAsync(doc.CustomerId, doc.DocumentId,
                        new PepCheckDocumentApproveRequest
                        {
                            Changer = SpiderChanger,
                            CheckResultComment = NoSuspectedProfiles,
                            CheckResultSatisfaction = true
                        });
                }

                await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(currentResultId, diff, doc));
            }

            if (state.Crime != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyByCrime(state.CheckResult);
                var doc = state.Crime;


                await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(UpdateDocuments),
                    $"Set status {doc.State} for DocumentId: {doc.DocumentId} for ClientId: {doc.CustomerId}");

                if (IsSuspectedDiff(diff))
                {
                    await _requestableDocumentsService.SubmitDocumentAsync(doc.CustomerId, doc.DocumentId, SpiderChanger);
                }
                else
                {
                    await _requestableDocumentsService.ApproveDocumentAsync(doc.CustomerId, doc.DocumentId,
                        new CrimeCheckDocumentApproveRequest
                        {
                            Changer = SpiderChanger,
                            CheckResultComment = NoSuspectedProfiles,
                            CheckResultSatisfaction = true
                        });
                }

                await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(currentResultId, diff, doc));
            }

            if (state.Sanction != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyBySanction(state.CheckResult);
                var doc = state.Sanction;

                await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(UpdateDocuments),
                    $"Set status {doc.State} for DocumentId: {doc.DocumentId} for ClientId: {doc.CustomerId}");

                doc.CheckDateTime = DateTime.UtcNow;
                await _typedDocumentsService.AddOrUpdateSanctionCheckDocumentAsync(doc);
                if (IsSuspectedDiff(diff))
                {
                    await _requestableDocumentsService.SubmitDocumentAsync(doc.CustomerId, doc.DocumentId, SpiderChanger);
                }
                else
                {
                    await _requestableDocumentsService.ApproveDocumentAsync(doc.CustomerId, doc.DocumentId,
                        new SanctionCheckDocumentApproveRequest()
                        {
                            Changer = SpiderChanger,
                            CheckResultComment = NoSuspectedProfiles,
                            CheckResultSatisfaction = true
                        });
                }

                await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(currentResultId, diff, doc));
            }
        }

        private static ISpiderDocumentInfo FormSpiderDocumentInfo(string currentCheckId, ISpiderCheckResultDiff diff, IKycDocumentInfo doc)
        {
            return new SpiderDocumentInfo
            {
                CustomerId = doc.CustomerId,
                DocumentId = doc.DocumentId,
                CheckDiff = Mapper.Map<SpiderCheckResultDiff>(diff),
                CurrentCheckId = currentCheckId,
                PreviousCheckId = null
            };
        }

        private static bool IsSuspectedDiff(ISpiderCheckResultDiff diff)
        {
            return diff.AddedProfiles.Count > 0 || diff.ChangedProfiles.Count > 0;
        }

        private class InstantCheckState
        {
            public ISpiderCheckResult CheckResult { get; set; }
            public PepCheckDocument Pep { get; set; }
            public CrimeCheckDocument Crime { get; set; }
            public SanctionCheckDocument Sanction { get; set; }
            public bool IsNewCustomer { get; set; }
        }

    }
}
