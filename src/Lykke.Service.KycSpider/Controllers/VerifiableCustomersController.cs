using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Controllers
{
    [Route("api/[controller]")]
    public class VerifiableCustomersController : Controller
    {
        private readonly IVerifiableCustomerService _verifiableCustomerService;
        private readonly IMapper _mapper;


        public VerifiableCustomersController
        (
            IVerifiableCustomerService verifiableCustomerService,
            IMapper mapper
        )
        {
            _verifiableCustomerService = verifiableCustomerService;
            _mapper = mapper;
        }

        [HttpGet("{clientId}")]
        [ProducesResponseType(typeof(VerifiableCustomerInfo), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAsync(string clientId)
        {
            var client = await _verifiableCustomerService.GetAsync(clientId);

            return Ok(_mapper.Map<VerifiableCustomerInfo>(client));
        }

        [HttpPost("disablecheck/{clientId}/pep")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DisablePepCheckAsync(string clientId)
        {
            await _verifiableCustomerService.DisablePepCheck(clientId);

            return Ok();
        }

        [HttpPost("disablecheck/{clientId}/crime")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DisableCrimeCheckAsync(string clientId)
        {
            await _verifiableCustomerService.DisableCrimeCheck(clientId);

            return Ok();
        }

        [HttpPost("disablecheck/{clientId}/sanction")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DisableSanctionCheckAsync(string clientId)
        {
            await _verifiableCustomerService.DisableSanctionCheck(clientId);

            return Ok();
        }
    }
}
