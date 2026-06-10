using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.OracleClient;

namespace CallAuditPortal1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)

        {
            _reportService = reportService;
        }
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            return Ok(await _reportService.GetBranches());
        }

        [HttpPost("feedback-status")]
        public async Task<IActionResult> GetFeedbackStatusReport([FromBody] FeedbackStatusRequest request)
        {
            var result = await _reportService.GetFeedbackStatusReport(request);
            return Ok(result);
        }

        [HttpPost("feedback-status/export")]
        public async Task<IActionResult> ExportFeedbackStatusReport([FromBody] FeedbackStatusRequest request)
        {
            var file = await _reportService.ExportFeedbackStatusReport(request);
            return File( file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"FeedbackStatus_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpPost("summary-status")]
        public async Task<IActionResult> GetSummaryStatusReport([FromBody] SummaryStatusRequest request)
        {
            var result = await _reportService.GetSummaryStatusReport(request);
            return Ok(result);
        }

        [HttpPost("summary-status/export")]
        public async Task<IActionResult> ExportSummaryStatusReport([FromBody] SummaryStatusRequest request)
        {
            var file = await _reportService.ExportSummaryStatusReport(request);
            return File(file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SummaryStatus_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

    }
}
