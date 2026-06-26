using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(string fromEmail, string toEmail, string ccEmail, string subject, string bodyHtml, string attachementName = null);

    }
}
