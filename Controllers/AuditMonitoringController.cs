using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service;
using CallAuditPortal1.Service.Helper;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System.Data;

namespace CallAuditPortal1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditMonitoringController : ControllerBase
    {
        private readonly IAuditMonitoringService _auditMonitoringService;
        private readonly IAuditMonitoringDAL _auditMonitoringDAL;
        private readonly RazorViewRenderer _razorRenderer;
        private readonly ILogger<AuditMonitoringController> _logger;
       

        public AuditMonitoringController(
            IAuditMonitoringService auditMonitoringService,
            //EmailService emailService,
            IAuditMonitoringDAL auditMonitoringDAL,
            RazorViewRenderer razorRenderer,
            ILogger<AuditMonitoringController> logger)
        {
            _auditMonitoringService = auditMonitoringService;
            _razorRenderer = razorRenderer;
            _auditMonitoringDAL = auditMonitoringDAL;
            _logger = logger;
           
        }

        [HttpPost("SubmitToBranch")]
        public async Task<IActionResult> SubmitToBranch([FromBody] SubmitBranchRequest request)

        {
            try
            {
                var result =
                    await _auditMonitoringService.SubmitToBranch(request);

                

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
        //[HttpPost("SubmitToBranch")]
        //public async Task<IActionResult> SubmitToBranch([FromBody] SubmitBranchRequest request)
        //{
        //    try
        //    {
        //        var result = await _auditMonitoringService.SubmitToBranch(request);
        //        var sendMailToBranch = await _auditMonitoringService.SendMailForSucessful("123", "Admin"); /*result.userid, result.role*/
        //        return Ok(new
        //        {
        //            success = true,
        //            message = result, 
        //            ismailSent = sendMailToBranch
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}

        //[HttpPost("SendMail")]
        //   public async Task<IActionResult> SendMail(string receiptNo)
        //   {
        //       try
        //       {
        //           string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

        //           var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
        //               receiptNo,
        //               out toEmail,
        //               out FromEmail,
        //               out CcEmail,
        //               out subject,
        //               out header,
        //               out footer,
        //               out attachementFile);

        //           // Hardcoded values for testing
        //           toEmail = "test@gmail.com";
        //           FromEmail = "noreply@lg.com";
        //           CcEmail = "test1@gmail.com";
        //           subject = "Test Mail From CS Audit Portal";
        //           attachementFile = null;

        //           var html = await _razorRenderer.RenderAsync(
        //"Email_Templates/Successful.cshtml",
        //new { });

        //           await _emailService.SendEmailAsync(
        //               fromEmail: FromEmail,
        //               toEmail: toEmail,
        //               ccEmail: CcEmail,
        //               subject: subject,
        //               bodyHtml: html,
        //               attachementName: attachementFile);

        //           return Ok("Mail Sent Successfully");
        //       }
        //       catch (Exception ex)
        //       {
        //           return StatusCode(500, ex.ToString());
        //       }
        //   }


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
            }
            catch (Exception ex)
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

