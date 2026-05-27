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
        [HttpPost("SearchAuditData")]
        public async Task<IActionResult> SearchAuditData([FromBody] AuditSearchRequest request)
        {
            var result = await _auditMonitoringService.SearchAuditData(request);
            return Ok(result);
        }

        [HttpPost("SubmitToBranch")]
        public async Task<IActionResult> SubmitToBranch([FromBody] SubmitBranchRequest request)
        {
            var result = await _auditMonitoringService.SubmitToBranch(request);
            return Ok(result);
        }
    }

}

