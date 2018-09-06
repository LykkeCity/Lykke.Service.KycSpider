using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Controllers
{
    [Route("api/[controller]")]
    public class SpiderController : Controller
    {
        private readonly ISpiderCheckManagerService _checkManagerService;
        private readonly ISpiderFirstCheckService _firstCheckService;

        public SpiderController
        (
            ISpiderCheckManagerService checkManagerService,
            ISpiderFirstCheckService firstCheckService
        )
        {
            _checkManagerService = checkManagerService;
            _firstCheckService = firstCheckService;
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

        [HttpGet("dofirstcheck/{clientId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DoFirstCheckAsync(string clientId)
        {
            await _firstCheckService.PerformFirstCheckAsync(clientId);

            return Ok();
        }
    }
}
