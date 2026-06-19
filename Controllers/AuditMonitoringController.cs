using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

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
            try
            {
                var result = await _auditMonitoringService.SubmitToBranch(request);
                return Ok(new
                {
                    success = true,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
       
        [HttpPost("Reject")]
        public async Task<IActionResult> Reject([FromBody] RejectRequest request)
        {
            try
            {
                var result = await _auditMonitoringService.Reject(request);
                return Ok(new
                {
                    success = true,
                    message = result
                });
            }catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }

        [HttpPost("Download")]
        public async Task<IActionResult> Download([FromBody] DownloadRequest request)
        {
            try
            {
                var fileBytes =
                    await _auditMonitoringService.Download(request);
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Audit_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
    }

}

