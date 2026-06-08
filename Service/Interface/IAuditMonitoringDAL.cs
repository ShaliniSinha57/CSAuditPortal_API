using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditMonitoringDAL
    {
        Task<string> SubmitToBranch(SubmitBranchRequest request);
        Task<string> Reject(RejectRequest request);
    }
}
