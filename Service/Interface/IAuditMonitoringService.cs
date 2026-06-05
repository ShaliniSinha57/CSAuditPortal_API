using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditMonitoringService
    {
        Task<List<AuditMonitoringModel>> SearchAuditData(AuditSearchRequest request);

        Task<string> SubmitToBranch(SubmitBranchRequest request);

        Task<string> Reject(RejectRequest request);

    }
}
