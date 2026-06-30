using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IMailProcessDAL
    {
        Task<List<MailResponseModel>> GetMailData(string screenType, string sessionId, string caseType);
        Task UpdateMailStatusAsync(string rowIds, string status, string msg);
    }
}
