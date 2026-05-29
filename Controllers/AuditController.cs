
using CallAuditPortal1.Service.BAL;
using CallAuditPortal1.Service.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using PuppeteerSharp;
using System.Data;
using System.Text.Json;

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
      try
      {
        var result = await _service.InsertDataIntoTempTable(fullPath, auditType, auditDate);
                return Ok(new
                {
                    status = "Success",
                    data = result
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


    }
}
