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

        //public async Task<byte[]> Download(DownloadRequest request)
        //{
        //    return await _auditMonitoringDAL.Download(request);
        //}
       

        public Task<byte[]> Download(DownloadRequest request)
        {
            return  _auditMonitoringDAL.Download(request);
        }

        public async Task<bool> SendMailForSucessful(string userid, string role)
        {
            bool isMailSend = false;
            try
            {
                string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

                //var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
                //   userid, role,
                //    out toEmail,
                //    out FromEmail,
                //    out CcEmail,
                //    out subject,
                //    out header,
                //    out footer,
                //    out attachementFile);

                // Hardcoded values for testing
                toEmail = "shalini.sinha@neuheittechnologies.com";
                FromEmail = "shalini.sinha@neuheittechnologies.com";
                CcEmail = "";
                subject = "Test Mail From CS Audit Portal";
                attachementFile = null;

                var html = await _razorRenderer.RenderAsync(
     "Email_Templates/Successful.cshtml",
     new { });

                await _emailService.SendEmailAsync(
                    fromEmail: FromEmail,
                    toEmail: toEmail,
                    ccEmail: CcEmail,
                    subject: subject,
                    bodyHtml: html,
                    attachementName: attachementFile);

                return isMailSend= true;
            }
            catch (Exception ex)
            {
                return isMailSend = false;
            }
        }

        public async Task<bool> SendMailForAccepted(string userid, string role)
        {
            bool isMailSend = false;
            try
            {
                string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

                //var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
                //   userid, role,
                //    out toEmail,
                //    out FromEmail,
                //    out CcEmail,
                //    out subject,
                //    out header,
                //    out footer,
                //    out attachementFile);

                // Hardcoded values for testing
                toEmail = "shalini.sinha@neuheittechnologies.com";
                FromEmail = "shalini.sinha@neuheittechnologies.com";
                CcEmail = "";
                subject = "Test Mail From CS Audit Portal";
                attachementFile = null;

                var html = await _razorRenderer.RenderAsync(
     "Email_Templates/Accepted_HO.cshtml",
     new { });

                await _emailService.SendEmailAsync(
                    fromEmail: FromEmail,
                    toEmail: toEmail,
                    ccEmail: CcEmail,
                    subject: subject,
                    bodyHtml: html,
                    attachementName: attachementFile);

                return isMailSend = true;
            }
            catch (Exception ex)
            {
                return isMailSend = false;
            }
        }

        public async Task<bool> SendMailForFeedback(string userid, string role)
        {
            bool isMailSend = false;
            try
            {
                string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

                //var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
                //   userid, role,
                //    out toEmail,
                //    out FromEmail,
                //    out CcEmail,
                //    out subject,
                //    out header,
                //    out footer,
                //    out attachementFile);

                // Hardcoded values for testing
                toEmail = "shalini.sinha@neuheittechnologies.com";
                FromEmail = "shalini.sinha@neuheittechnologies.com";
                CcEmail = "";
                subject = "Test Mail From CS Audit Portal";
                attachementFile = null;

                var html = await _razorRenderer.RenderAsync(
     "Email_Templates/Feedback.cshtml",
     new { });

                await _emailService.SendEmailAsync(
                    fromEmail: FromEmail,
                    toEmail: toEmail,
                    ccEmail: CcEmail,
                    subject: subject,
                    bodyHtml: html,
                    attachementName: attachementFile);

                return isMailSend = true;
            }
            catch (Exception ex)
            {
                return isMailSend = false;
            }
        }
        public async Task<bool> SendMailForRejected(string userid, string role)
        {
            bool isMailSend = false;
            try
            {
                string toEmail, FromEmail, CcEmail, subject, header, footer, attachementFile;

                //var result = _auditMonitoringDAL.SubmitToBranchSendEmail(
                //   userid, role,
                //    out toEmail,
                //    out FromEmail,
                //    out CcEmail,
                //    out subject,
                //    out header,
                //    out footer,
                //    out attachementFile);

                // Hardcoded values for testing
                toEmail = "shalini.sinha@neuheittechnologies.com";
                FromEmail = "shalini.sinha@neuheittechnologies.com";
                CcEmail = "";
                subject = "Test Mail From CS Audit Portal";
                attachementFile = null;

                var html = await _razorRenderer.RenderAsync(
     "Email_Templates/Rejected_HO.cshtml",
     new { });

                await _emailService.SendEmailAsync(
                    fromEmail: FromEmail,
                    toEmail: toEmail,
                    ccEmail: CcEmail,
                    subject: subject,
                    bodyHtml: html,
                    attachementName: attachementFile);

                return isMailSend = true;
            }
            catch (Exception ex)
            {
                return isMailSend = false;
            }
        }

    }


}
