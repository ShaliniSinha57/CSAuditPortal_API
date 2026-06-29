using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CallAuditPortal1.Service
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }
        public async Task SendEmailAsync(string fromEmail, string toEmail, string ccEmail, string subject, string bodyHtml, string attachementName = null)
        {
            try
            {
                var introMessage = @"<p>Dear User,</p> <p>This is the scheme that has been approved:</p>        ";
                var footer = @"<p style='font-size:12px; color:#777;'>This is an automated message from LG CS AuditPortal </p>";
                //var finalBody = introMessage + footer;
                var finalBody = introMessage + bodyHtml + footer;
                using var smtp = new SmtpClient(_smtpSettings.Host)
                {
                    Port = _smtpSettings.Port > 0 ? _smtpSettings.Port : 25,
                    Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl,
                };
                var message = new MailMessage
                {
                    From = new MailAddress(string.IsNullOrEmpty(fromEmail) ? _smtpSettings.FromEmail : fromEmail, _smtpSettings.DisplayName),
                    Subject = subject,
                    Body = finalBody,
                    IsBodyHtml = true,
                };
                if (!string.IsNullOrEmpty(attachementName) && System.IO.File.Exists(attachementName))
                {
                    string uploadsFolder = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/AttachemeFile/");
                   // var attachmentFileName = uploadsFolder + "" + attachementName;
                    var attachmentFileName = Path.Combine(uploadsFolder, attachementName);
                    var attachment = new Attachment(attachmentFileName);
                    message.Attachments.Add(attachment);
                }
                message.To.Add(toEmail);
                if (!string.IsNullOrEmpty(ccEmail)) message.CC.Add(ccEmail);
                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public async Task<string> GetTemplate(string process)
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
    }
}