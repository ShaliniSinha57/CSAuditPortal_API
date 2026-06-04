
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


    [HttpGet("DownloadTemplate")]
    public IActionResult DownloadTemplate()
    {
      try
      {
        var data = _auditBAL.DownloadTemplate();

        return Ok(data);
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
    
    [HttpPost("ProcessUploadData")]
    public async Task<IActionResult> ProcessUploadData(string fullPath, string auditType, string auditDate)
    {
            string sessionid = string.Empty;
      try
      {
        var data = await _service.InsertDataIntoTempTable(fullPath, auditType, auditDate);
                return Ok(new
                {
                    status = "Success",
                    data = data.result,
                    sessionId = data.session_Id
                });
            }
      catch (Exception)
      {

        throw;
      }
    }
        [HttpGet("verify-upload")]
        public async Task<IActionResult> VerifyUpload(string sessionId, string templateId)


        {
            try
            {
                var data = await _service.VerifyUpload(sessionId, templateId);

                return Ok(new
                {
                    success = true,
                    data = data
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
            var result = await _service.SaveStatus(request);
            return Ok(new 
            {
                message = "Data Saved Successfully"
            });
        }

        [HttpPost("reject-status")]
        public async Task<IActionResult> RejectStatus([FromBody] RejectUploadedDataRequest request)
        {
            var result = await _service.RejectStatus(request);
            return Ok(new 
            { 
                message = "File Rejected Successfully"
            }
            );
        }


    }
}
