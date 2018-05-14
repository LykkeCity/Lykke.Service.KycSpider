using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
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
        private readonly ILog _log;

        public SpiderInstantCheckService
        (
            ICheckPersonResultDiffService diffService,
            ISpiderDocumentInfoRepository spiderDocumentInfoRepository,
            IVerifiableCustomerInfoRepository verifiableCustomerRepository,
            ITypedDocumentsService typedDocumentsService,
            ISpiderCheckService spiderCheckService,
            ISpiderCheckResultRepository spiderCheckResultRepository,
            ILog log
        )
        {
            _diffService = diffService;
            _spiderDocumentInfoRepository = spiderDocumentInfoRepository;
            _verifiableCustomerRepository = verifiableCustomerRepository;
            _typedDocumentsService = typedDocumentsService;
            _spiderCheckService = spiderCheckService;
            _spiderCheckResultRepository = spiderCheckResultRepository;
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

                if (spiderCheckId != null)
                {
                    state.CheckResult = await _spiderCheckResultRepository.GetAsync(clientId, spiderCheckId);
                }
                else
                {
                    state.CheckResult = await _spiderCheckService.CheckAsync(clientId);
                }

                await UpdateDocuments(state);
                await UpdateVerifiableCustomer(state);
            }

            await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(PerformInstantCheckAsync), "done");
        }

        private async Task UpdateVerifiableCustomer(InstantCheckState state)
        {
            var clientId = state.CheckResult.CustomerId;
            var oldClient = await _verifiableCustomerRepository.GetAsync(clientId) ??
                            new VerifiableCustomerInfo { CustomerId = clientId };

            var newClient = new VerifiableCustomerInfo
            {
                CustomerId = oldClient.CustomerId,
                IsCrimeCheckRequired = oldClient.IsCrimeCheckRequired || state.Crime != null,
                IsSanctionCheckRequired = oldClient.IsSanctionCheckRequired || state.Sanction != null,
                IsPepCheckRequired = oldClient.IsPepCheckRequired || state.Pep != null,
                LatestSpiderCheckId = state.CheckResult.ResultId
            };

            await _verifiableCustomerRepository.AddOrUpdateAsync(newClient);
        }

        private async Task UpdateDocuments(InstantCheckState state)
        {
            var currentResultId = state.CheckResult.ResultId;

            if (state.Pep != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyByPep(state.CheckResult);
                var doc = state.Pep;

                if (IsSuspectedDiff(diff))
                {
                    doc.State = DocumentStates.Uploaded;
                }
                else
                {
                    doc.State = DocumentStates.Approved;
                    doc.CheckResultSatisfaction = true;
                }

                doc.CheckDateTime = DateTime.UtcNow;

                await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(UpdateDocuments),
                    $"Set status {doc.State} for DocumentId: {doc.DocumentId} for ClientId: {doc.CustomerId}");
                await _typedDocumentsService.AddOrUpdatePepCheckDocumentAsync(doc);
                await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(currentResultId, diff, doc));
            }

            if (state.Crime != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyByCrime(state.CheckResult);
                var doc = state.Crime;

                if (IsSuspectedDiff(diff))
                {
                    doc.State = DocumentStates.Uploaded;
                }
                else
                {
                    doc.State = DocumentStates.Approved;
                    doc.CheckResultSatisfaction = true;
                }

                doc.CheckDateTime = DateTime.UtcNow;
                await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(UpdateDocuments),
                    $"Set status {doc.State} for DocumentId: {doc.DocumentId} for ClientId: {doc.CustomerId}");
                await _typedDocumentsService.AddOrUpdateCrimeCheckDocumentAsync(doc);
                await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(currentResultId, diff, doc));
            }

            if (state.Sanction != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyBySanction(state.CheckResult);
                var doc = state.Sanction;

                if (IsSuspectedDiff(diff))
                {
                    doc.State = DocumentStates.Uploaded;
                }
                else
                {
                    doc.State = DocumentStates.Approved;
                    doc.CheckResultSatisfaction = true;
                }

                doc.CheckDateTime = DateTime.UtcNow;
                await _log.WriteInfoAsync(nameof(SpiderInstantCheckService), nameof(UpdateDocuments),
                    $"Set status {doc.State} for DocumentId: {doc.DocumentId} for ClientId: {doc.CustomerId}");
                await _typedDocumentsService.AddOrUpdateSanctionCheckDocumentAsync(doc);
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
        }

    }
}
