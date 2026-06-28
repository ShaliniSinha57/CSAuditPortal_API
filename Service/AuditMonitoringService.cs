using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Helper;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CallAuditPortal1.Service
{
    public class AuditMonitoringService : IAuditMonitoringService
    {
        private readonly IAuditMonitoringDAL _auditMonitoringDAL;
        private readonly IEmailService _emailService;
        private readonly RazorViewRenderer _razorRenderer;

        public AuditMonitoringService(IAuditMonitoringDAL auditMonitoringDAL, IEmailService emailService, RazorViewRenderer razorRenderer)
        {
            _auditMonitoringDAL = auditMonitoringDAL;
            _emailService = emailService;
            _razorRenderer = razorRenderer;
        }

        public async Task<string> SubmitToBranch(SubmitBranchRequest request)
        {
            return await _auditMonitoringDAL.SubmitToBranch(request);
        }
        
        public async Task<string> Reject(RejectRequest request)
        {
            return await _auditMonitoringDAL.Reject(request);
        }

        public Task<byte[]> Download(DownloadRequest request)
        {
            return  _auditMonitoringDAL.Download(request);
        }

        //   public async Task<bool> SendMailForSucessful(string userid, string role)
        //   {
        //       bool isMailSend = false;
        //       try
        //       {
        //           string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

        //           //var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
        //           //   userid, role,
        //           //    out toEmail,
        //           //    out FromEmail,
        //           //    out CcEmail,
        //           //    out subject,
        //           //    out header,
        //           //    out footer,
        //           //    out attachementFile);

        //           // Hardcoded values for testing
        //           toEmail = "shalini.sinha@neuheittechnologies.com";
        //           FromEmail = "shalini.sinha@neuheittechnologies.com";
        //           CcEmail = "";
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

        //           return isMailSend= true;
        //       }
        //       catch (Exception ex)
        //       {
        //           return isMailSend = false;
        //       }
        //   }

        //   public async Task<bool> SendMailForAccepted(string userid, string role)
        //   {
        //       bool isMailSend = false;
        //       try
        //       {
        //           string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

        //           //var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
        //           //   userid, role,
        //           //    out toEmail,
        //           //    out FromEmail,
        //           //    out CcEmail,
        //           //    out subject,
        //           //    out header,
        //           //    out footer,
        //           //    out attachementFile);

        //           // Hardcoded values for testing
        //           toEmail = "shalini.sinha@neuheittechnologies.com";
        //           FromEmail = "shalini.sinha@neuheittechnologies.com";
        //           CcEmail = "";
        //           subject = "Test Mail From CS Audit Portal";
        //           attachementFile = null;

        //           var html = await _razorRenderer.RenderAsync(
        //"Email_Templates/Accepted_HO.cshtml",
        //new { });

        //           await _emailService.SendEmailAsync(
        //               fromEmail: FromEmail,
        //               toEmail: toEmail,
        //               ccEmail: CcEmail,
        //               subject: subject,
        //               bodyHtml: html,
        //               attachementName: attachementFile);

        //           return isMailSend = true;
        //       }
        //       catch (Exception ex)
        //       {
        //           return isMailSend = false;
        //       }
        //   }

        //   public async Task<bool> SendMailForFeedback(string userid, string role)
        //   {
        //       bool isMailSend = false;
        //       try
        //       {
        //           string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

        //           //var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
        //           //   userid, role,
        //           //    out toEmail,
        //           //    out FromEmail,
        //           //    out CcEmail,
        //           //    out subject,
        //           //    out header,
        //           //    out footer,
        //           //    out attachementFile);

        //           // Hardcoded values for testing
        //           toEmail = "shalini.sinha@neuheittechnologies.com";
        //           FromEmail = "shalini.sinha@neuheittechnologies.com";
        //           CcEmail = "";
        //           subject = "Test Mail From CS Audit Portal";
        //           attachementFile = null;

        //           var html = await _razorRenderer.RenderAsync(
        //"Email_Templates/Feedback.cshtml",
        //new { });

        //           await _emailService.SendEmailAsync(
        //               fromEmail: FromEmail,
        //               toEmail: toEmail,
        //               ccEmail: CcEmail,
        //               subject: subject,
        //               bodyHtml: html,
        //               attachementName: attachementFile);

        //           return isMailSend = true;
        //       }
        //       catch (Exception ex)
        //       {
        //           return isMailSend = false;
        //       }
        //   }
        //   public async Task<bool> SendMailForRejected(string userid, string role)
        //   {
        //       bool isMailSend = false;
        //       try
        //       {
        //           string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

        //           //var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
        //           //   userid, role,
        //           //    out toEmail,
        //           //    out FromEmail,
        //           //    out CcEmail,
        //           //    out subject,
        //           //    out header,
        //           //    out footer,
        //           //    out attachementFile);

        //           // Hardcoded values for testing
        //           toEmail = "shalini.sinha@neuheittechnologies.com";
        //           FromEmail = "shalini.sinha@neuheittechnologies.com";
        //           CcEmail = "";
        //           subject = "Test Mail From CS Audit Portal";
        //           attachementFile = null;

        //           var html = await _razorRenderer.RenderAsync(
        //"Email_Templates/Rejected_HO.cshtml",
        //new { });

        //           await _emailService.SendEmailAsync(
        //               fromEmail: FromEmail,
        //               toEmail: toEmail,
        //               ccEmail: CcEmail,
        //               subject: subject,
        //               bodyHtml: html,
        //               attachementName: attachementFile);

        //           return isMailSend = true;
        //       }
        //       catch (Exception ex)
        //       {
        //           return isMailSend = false;
        //       }
        //   }
        public async Task<bool> SendMailByScreenType(
       string userId,
       string process,
       string sessionId,
       string receiptNos = "")
        {
            try
            {
                // Step 1 : Generate Mail Data
                var result = await _auditMonitoringDAL.GenerateMailData(
                    userId,
                    process,
                    sessionId,
                    receiptNos);

                if (!result.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                    return false;

                // Step 2 : Read Mail Data
                var mailList = await _auditMonitoringDAL.GetMailExcelData(
                    process,
                    sessionId);

                foreach (var item in mailList)
                {
                    var emailModel = new Email
                    {
                        To = item.MailTo,
                        CC = item.MailCc,
                        MailSubject = item.Subject,
                        Header = item.Header,
                        Footer = item.Footer,
                        ShipToCode = item.ShipToCode,
                        CompanyName = item.CompanyName,
                        AuditMonth = item.AuditMonth,
                        LastDate = item.ValidTill,
                        Rows = item.Rows
                    };

                    string template = GetTemplate(process);

                    string html = await _razorRenderer.RenderAsync(
                        template,
                        emailModel);

                    await _emailService.SendEmailAsync(
                        fromEmail: "",
                        toEmail: emailModel.To,
                        ccEmail: emailModel.CC,
                        subject: string.IsNullOrWhiteSpace(item.Subject)
                                    ? GetSubject(process)
                                    : item.Subject,
                        bodyHtml: html,
                        attachementName: item.AttachmentFile);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private string GetTemplate(string process)
        {
            return process switch
            {
                "SUBMIT_PROCESS" => "Email_Templates/Successful.cshtml",
                "HO_ACCEPT" => "Email_Templates/Accepted_HO.cshtml",
                "HO_REJECT" => "Email_Templates/Rejected_HO.cshtml",
                "FEEDBACK_ESC_LGC" => "Email_Templates/Feedback.cshtml",
                _ => throw new Exception("Invalid Process")
            };
        }

        private string GetSubject(string process)
        {
            return process switch
            {
                "SUBMIT_PROCESS" => "Audit Submitted Successfully",
                "HO_ACCEPT" => "Audit Accepted",
                "HO_REJECT" => "Audit Rejected",
                "FEEDBACK_ESC_LGC" => "Feedback Submitted",
                _ => "Audit Mail"
            };
        }
    }


}
