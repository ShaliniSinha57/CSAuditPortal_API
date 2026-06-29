using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using System.Data;

namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditMonitoringService
    {
        //Task<string> SubmitToBranch(SubmitBranchRequest request);
        Task<string> SubmitToBranch(SubmitBranchRequest request);
        Task<bool> SendMailByScreenType(string userId, string process, string sessionId, string receiptNos = "");
        Task<string> Reject(RejectRequest request);
        Task<byte[]> Download(DownloadRequest request);
       
    }
}
