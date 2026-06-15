
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.BAL;
using CallAuditPortal1.Service.Interface;
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


           
        
        [HttpGet("download-template/{auditTypeId}")]
        public async Task<IActionResult> DownloadTemplate(int auditTypeId)
        {
            try
            {
                string filePath = await _service.DownloadTemplate(auditTypeId);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Template file not found at path: {filePath}"
                    });
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                string fileName = Path.GetFileName(filePath);

                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("ProcessUploadData")]
            public async Task<IActionResult> ProcessUploadData([FromForm] AuditUploadClaimRequest request)
            {
                try
                {
                var data = await _service.InsertDataIntoTempTable(request);
                    return Ok(new
                    {
                        status = "Success",
                        data = data.result,
                        sessionId = data.session_Id
                    });
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
