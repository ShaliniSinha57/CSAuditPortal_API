using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using OfficeOpenXml;



namespace CallAuditPortal1.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class FileUploadController : ControllerBase
  {
    private readonly IConfiguration _configuration;
    public FileUploadController(IConfiguration configuration)
    {
      _configuration = configuration;
    }



    [HttpPost("UploadAudittemplate")]
    [RequestSizeLimit(52428800)] // Set limit to 50 MB
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
    [AllowAnonymous]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string auditType)
    {
      if (file == null || file.Length == 0)
      {
        return BadRequest("File not selected or is empty.");
      }

      try
      {
        Dictionary<string, string> templateFolders =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "1", "AMC_Incentive_Hold" },
            { "2", "Beyond_AMC_Policy" },
            { "3", "Claims_Self_Registration" },
            { "4", "Data_Audit" },
            { "5", "Gas_Overcharging" },
            { "6", "Duplicate_Data" },
            { "7", "Estimate_OW_AMC" },
            { "8", "Extra_Filter_AMC" },
            { "9", "Inside_Filters_IW_Wty" },
            { "10", "Model_Change" },
            { "11", "OOW_Claims" },
            { "12", "Part_in_Labour" },
            { "13", "Post_AMC_30_Days" },
            { "14", "RF_Special_Model" },
            { "15", "Suspicious_AMC" },
            { "16", "Wrong_Part_Consumption" },
            { "17", "Multiple_closure_same_day_same" },
            { "18", "LGC_Part_Rejection" },
            { "19", "LGC_Non_Part_Rejection" },
            { "20", "Cancellation_Claim_Recovery" },
            { "21", "Multiple_Bracket" }
        };
        // Check auditType exists
        if (!templateFolders.TryGetValue(auditType, out string folderName))
        {
          return BadRequest("Invalid audit/template type.");
        }
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "AuditClaimUpload",
     folderName);

        if (!Directory.Exists(uploadsFolder))
        {
          Directory.CreateDirectory(uploadsFolder);
        }

        var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{auditType}_{originalFileName}{fileExtension}";
        var fullPath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
          await file.CopyToAsync(stream);
        }
        return Ok(new { Status = 200, message = "File uploaded successfully", fileName = fileName, fullPath = fullPath });
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

  }
}
