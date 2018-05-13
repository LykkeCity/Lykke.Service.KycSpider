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
        private readonly IMapper _mapper;

        public SpiderDocumentsController
        (
            ISpiderDocumentsService spiderDocumentsService,
            IMapper mapper
        )
        {
            _spiderDocumentsService = spiderDocumentsService;
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
    }
}
