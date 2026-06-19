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
       
    }
}
