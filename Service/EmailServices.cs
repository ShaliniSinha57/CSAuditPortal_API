using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Helper;
using CallAuditPortal1.Service.Interface;
using Microsoft.Extensions.Options;

namespace CallAuditPortal1.Service
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly RazorViewRenderer _razorRenderer;
        public EmailService(IOptions<SmtpSettings> smtpSettings, RazorViewRenderer razorViewRenderer)
        {
            _smtpSettings = smtpSettings.Value;
            _razorRenderer = razorViewRenderer;
        }
        public async Task SendEmailAsync(Email email, string ProcessCode)
        {
            try
            {
                string template = await GetTemplate(ProcessCode);
                string html = await _razorRenderer.RenderAsync(
                            template,
                            email);
                var introMessage = @"<p>Dear User,</p> <p>This is the scheme that has been approved:</p>        ";
                var footer = @"<p style='font-size:12px; color:#777;'>This is an automated message from LG CS AuditPortal </p>";
               
                var finalBody = introMessage + html + footer;
                using var smtp = new SmtpClient(_smtpSettings.Host)
                {
                    Port = _smtpSettings.Port > 0 ? _smtpSettings.Port : 25,
                    Credentials = new NetworkCredential(_smtpSettings.UserName , _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl,
                };

                var message = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.UserName, _smtpSettings.DisplayName),
                    Subject = email.MailSubject,
                    Body = finalBody,
                    IsBodyHtml = true,
                };

                if (!string.IsNullOrWhiteSpace(email.To))
                {
                    foreach (var item in email.To.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        message.To.Add(new MailAddress(item));
                    }
                }

                if (!string.IsNullOrWhiteSpace(email.CC))
                {
                    foreach(var item in email.CC.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        message.CC.Add(item);
                    }
                }
                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private async Task<string> GetTemplate(string process)
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