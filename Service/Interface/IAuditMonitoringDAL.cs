using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using System.Data;

namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditMonitoringDAL
    {
        Task<(string msg, string sessionId, string successReceipt, string errorReceipt)> SubmitToBranch(SubmitBranchRequest request);
        Task<string> Reject(RejectRequest request);
        Task<byte[]> Download(DownloadRequest request);
        Task<List<MailResponseModel>> GetMailExcelData(string screenType, string sessionId);
    }
}
