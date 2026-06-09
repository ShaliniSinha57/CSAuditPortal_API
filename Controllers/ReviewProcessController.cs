using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.DAL;
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
            var result = await _services.SearchReviewProcess(request);
            return Ok(result);
        }
        

        [HttpPost("Download")]
        public async Task<IActionResult> Download([FromBody] DownloadReviewProcessRequest request)
        {
            var result = await _services.DownloadReviewProcess(request);
            return Ok(result);
        }

    }
}
