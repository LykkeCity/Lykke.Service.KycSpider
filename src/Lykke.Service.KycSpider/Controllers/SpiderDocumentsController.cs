using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Controllers
{
    [Route("api/[controller]")]
    public class SpiderDocumentsController : Controller
    {
        private readonly ISpiderDocumentsService _spiderDocumentsService;
        private readonly ISpiderCheckManagerService _checkManagerService;
        private readonly IMapper _mapper;

        public SpiderDocumentsController
        (
            ISpiderDocumentsService spiderDocumentsService,
            ISpiderCheckManagerService checkManagerService,
            IMapper mapper
        )
        {
            _spiderDocumentsService = spiderDocumentsService;
            _checkManagerService = checkManagerService;
            _mapper = mapper;
        }

        [HttpGet("{clientId}/{documentId}")]
        [ProducesResponseType(typeof(SpiderDocumentInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> GetSpiderDocumentInfoAsync(string clientId, string documentId)
        {
            var info = await _spiderDocumentsService.GetSpiderDocumentInfoAsync(clientId, documentId);

            if (info == null)
            {
                NoContent();
            }
            return Ok(_mapper.Map<SpiderDocumentInfo>(info));
        }

        [HttpGet("doinstantcheck")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> RunInstantCheckAsync()
        {
            var isStarted = await _checkManagerService.TryStartInstantCheckManuallyAsync();

            if (!isStarted)
            {
                return Ok("Instant check did not start manually due to previous task in progress");
            }

            return Ok("Instant check started manually");
        }

        [HttpGet("startregulalcheck")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> RunRegularCheckAsync()
        {
            var isStarted = await _checkManagerService.TryStartRegularCheckManuallyAsync();

            if (!isStarted)
            {
                return Ok("Regular check did not start manually due to previous task in progress");
            }

            return Ok("Regular check started manually");
        }
    }
}
