using System.Net;
using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Microsoft.AspNetCore.Mvc;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Controllers
{
    [Route("api/[controller]")]
    public class SpiderManageController : Controller
    {
        private readonly ISpiderCheckManagerService _checkManagerService;
        private readonly ISpiderFirstCheckService _firstCheckService;

        public SpiderManageController
        (
            ISpiderCheckManagerService checkManagerService,
            ISpiderFirstCheckService firstCheckService
        )
        {
            _checkManagerService = checkManagerService;
            _firstCheckService = firstCheckService;
        }


        [HttpPost("startregulalcheck")]
        public async Task<IActionResult> StartRegularCheckAsync()
        {
            var isStarted = await _checkManagerService.TryStartRegularCheckManuallyAsync();

            if (!isStarted)
            {
                return Ok("Regular check did not start manually due to previous task in progress");
            }

            return Ok("Regular check started manually");
        }

        [HttpPost("dofirstcheck/{clientId}")]
        public async Task DoFirstCheckAsync(string clientId)
        {
            await _firstCheckService.PerformFirstCheckAsync(clientId);
        }

        [HttpPost("movefirstcheck/{clientId}")]
        public async Task<SpiderDocumentAutoStatusGroup> MoveFirstCheckAsync(string clientId, [FromBody]SpiderCheckResult spiderResult)
        {
            return await _firstCheckService.PerformFirstCheckAsync(clientId, spiderResult);
        }
    }
}
