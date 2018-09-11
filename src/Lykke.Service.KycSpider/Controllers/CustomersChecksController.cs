using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Controllers
{
    [Route("api/[controller]")]
    public class CustomersChecksController : Controller
    {
        private readonly ICustomerChecksService _customerChecksService;
        private readonly IMapper _mapper;


        public CustomersChecksController
        (
            ICustomerChecksService customerChecksService,
            IMapper mapper
        )
        {
            _customerChecksService = customerChecksService;
            _mapper = mapper;
        }

        [HttpGet("getchecksinfo/{clientId}")]
        public async Task<CustomerChecksInfo> GetChecksInfoAsync(string clientId)
        {
            var client = await _customerChecksService.GetAsync(clientId);

            return _mapper.Map<CustomerChecksInfo>(client);
        }

        [HttpGet("getdocumentinfo/{clientId}/{documentId}")]
        public async Task<SpiderDocumentInfo> GetDocumentInfoAsync(string clientId, string documentId)
        {
            var info = await _customerChecksService.GetSpiderDocumentInfo(clientId, documentId);

            return _mapper.Map<SpiderDocumentInfo>(info);
        }

        [HttpPost("enablecheck/{clientId}/{type}")]
        public async Task EnableCheckAsync(string clientId, string type)
        {
            await _customerChecksService.EnableCheck(clientId, type);
        }
    }
}
