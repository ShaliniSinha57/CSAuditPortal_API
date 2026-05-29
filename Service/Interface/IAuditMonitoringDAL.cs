using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditMonitoringDAL
    {
        Task<List<AuditMonitoringModel>> SearchAuditData(AuditSearchRequest request);
        Task<string> SubmitToBranch(SubmitBranchRequest request);
        Task<string> Download(DownloadRequest request);
        Task<string> Reject(RejectRequest request);
    }
}
