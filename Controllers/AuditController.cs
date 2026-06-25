
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.BAL;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;

namespace CallAuditPortal1.Controllers
{
      [Route("api/[controller]")]
      [ApiController]
      public class AuditController : ControllerBase
      {
            private readonly AuditBAL _auditBAL;
            private readonly IDataLoaderService _service;
            public AuditController(AuditBAL auditBAL, IDataLoaderService service)
            {
                _auditBAL = auditBAL;
                _service = service;
            }


            [HttpGet("GetAuditDropdownList")]
            public IActionResult GetAuditDropdownList()
            {
                var data = _auditBAL.GetDropdown();
                return Ok(data);
            }


            [HttpPost("DownloadTemplate")]
            public async Task<IActionResult> DownloadTemplate(int AuditType)
            {
                try
                {
                    var data = await _auditBAL.DownloadTemplate(AuditType);
                if (data.Item1 == null)
                {
                    return NotFound("File Not Found");
                }

                return File(data.Item1,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{data.Item2}.xlsx"
                    );
                }
                catch (Exception ex)
                {
                return BadRequest(ex.Message);
                }
            }

        [HttpPost("DownloadExcelErrorRow")]
        public async Task<IActionResult> DownloadExcelErrorRow(int templateId, int sessionId)
        {
            try
            {
                var data = await _service.DownloadErrorRows(templateId, sessionId);
                if (data.Item1 == null)
                {
                    return NotFound("File Not Found");
                }

                return File(data.Item1,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{data.Item2}.xlsx"
                    );
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ProcessUploadData")]
            [RequestSizeLimit(100_000_000)]
            [RequestFormLimits(MultipartBodyLengthLimit = 100_000_000)]
            public async Task<IActionResult> ProcessUploadData([FromForm] AuditUploadClaimRequest request)
            {
                try
                {
                    var data = await _service.InsertDataIntoTempTable(request);
                    if (data.Status)
                    {
                        return Ok(new
                        {
                            status = "Success",
                            data = data,
                        });
                    }
                    else
                    {
                        return StatusCode(400, new
                        {
                            data
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }
            [HttpPost("search-audit")]
            public async Task<IActionResult> SearchAuditData([FromBody] AuditSearchRequest request)
            {
                try
                {
                    var data = await _service.SearchAuditData(request);

                    return Ok(new
                    {
                        success = true,
                        totalRecords = data.Item1,
                        data = data.Item2
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }


            [HttpPost("save-status")]
            public async Task<IActionResult> SaveStatus([FromBody] SaveStatusRequest request)
            {
                try
                {
                    var result = await _service.SaveStatus(request);
                    return Ok(new
                    {
                        message = "Data Saved Successfully"
                    });
                }
                catch(Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            
            }

            [HttpPost("reject-status")]
            public async Task<IActionResult> RejectStatus([FromBody] RejectUploadedDataRequest request)
            {
                try
                {
                    var result = await _service.RejectStatus(request);
                    return Ok(new
                    {
                        message = "File Rejected Successfully"
                    }
                    );
                }
               catch(Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
            }


        }
}
