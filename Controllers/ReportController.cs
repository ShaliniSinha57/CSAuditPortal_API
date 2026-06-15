using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service;
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
        private readonly IReportDAL _reportService;

        public ReportController(IReportDAL reportService)

        {
            _reportService = reportService;
        }
        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var branches = await _reportService.GetBranches();

                return Ok(branches);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "Error while fetching branches.",
                        Error = ex.Message
                    });
            }
        }

        [HttpPost("SearchFeedbackStatus")]
        public async Task<IActionResult> SearchFeedbackStatus([FromBody] FeedbackStatusRequest request)
        {
            try
            {
                var (count, data) =
                    await _reportService
                        .SearchFeedbackStatusReport(request);

                return Ok(new
                {
                    TotalCount = count,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("DownloadFeedbackReport")]
        public async Task<IActionResult> DownloadFeedbackReport([FromBody] ExportExcel excel)
        {
            try
            {
                byte[] fileBytes =
                    await _reportService.DownloadFeedbackReport(excel);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"FeedbackStatusReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpPost("SearchSummaryStatus")]
        public async Task<IActionResult> SearchSummaryStatusReport(
    [FromBody] SummaryStatusRequest request)
        {
            try
            {
                var result = await _reportService
                    .SearchSummaryStatusReport(request);

                return Ok(result);
                //return Ok(new
                //{
                //    data = result, 
                //    status = 200, 
                //    msg = " Data Fetech Sucessfully"
                //});
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "An error occurred while fetching summary status report.",
                        Error = ex.Message
                    });
            }
        }


        [HttpPost("DownloadSummaryStatus")]
        public async Task<IActionResult> DownloadSummaryStatusReport(
  [FromBody] ExportSummaryExcel export)
        {
            try
            {
                byte[] file = await _reportService
                    .DownloadSummaryStatusReport(export);

                return File(
                    file,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"SummaryStatus_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        Message = "Error while exporting summary status report.",
                        Error = ex.Message
                    });
            }
        }
    }
}
        

