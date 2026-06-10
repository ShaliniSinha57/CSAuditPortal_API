using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CallAuditPortal1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewProcessController : ControllerBase
    {
        private readonly IReviewProcessDAL _services;

        public ReviewProcessController(IReviewProcessDAL services)
        {
            _services = services;
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] ReviewProcessSearchRequest request)
        {
            try
            {
                var result = await _services.SearchReviewProcess(request);
                return Ok(new
                {
                    success = true,
                    totalData = result.Item1,
                    data = result.Item2
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }

        [HttpPost("Download")]
        public async Task<IActionResult> Download([FromBody] ReviewProcessSearchRequest request)
        {
            try
            {
                var fileBytes = await _services.DownloadReviewProcess(request);
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Audit_Review_Search_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch(Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

    }
}
