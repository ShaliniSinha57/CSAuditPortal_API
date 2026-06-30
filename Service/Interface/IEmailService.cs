using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IEmailService
    {
        Task SendEmailAsync(Email mail, string processCode);

    }
}
