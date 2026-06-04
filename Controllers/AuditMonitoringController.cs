using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CallAuditPortal1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditMonitoringController : ControllerBase
    {

        private readonly IAuditMonitoringService _auditMonitoringService;

        public AuditMonitoringController(IAuditMonitoringService auditMonitoringService)


        {
            _auditMonitoringService = auditMonitoringService;
        }
        

        [HttpPost("SubmitToBranch")]
        public async Task<IActionResult> SubmitToBranch([FromBody] SubmitBranchRequest request)
        {
            var result = await _auditMonitoringService.SubmitToBranch(request);
            return Ok(result);
        }
        [HttpPost("Download")]
        public async Task<IActionResult> Download([FromBody] DownloadRequest request)
        {
            var result = await _auditMonitoringService.Download(request);
            return Ok(result);
        }
        [HttpPost("Reject")]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            var result = await _auditMonitoringService.Reject(request);
            return Ok(result);
        }


    }

}

