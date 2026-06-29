using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using System.Data;

namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditMonitoringDAL
    {
        Task<string> SubmitToBranch(SubmitBranchRequest request);
        Task<string> Reject(RejectRequest request);
        Task<byte[]> Download(DownloadRequest request);
        string SubmitToBranchSendEmail(string userId, string process, string sessionId, string receiptNos, out string FromEmail, out string toEmail, out string CcEmail, out string subject, out string header, out string footer, out string attachementFile);
        Task<List<MailResponseModel>> GetMailExcelData(string screenType, string sessionId);
        Task<string> GenerateMailData(string userId, string process, string sessionId, string receiptNos);
    }
}
