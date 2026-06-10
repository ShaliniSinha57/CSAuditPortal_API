using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;

namespace CallAuditPortal1.Service
{
    public class AuditMonitoringService : IAuditMonitoringService
    {
        private readonly IAuditMonitoringDAL _auditMonitoringDAL;

        public AuditMonitoringService(IAuditMonitoringDAL auditMonitoringDAL)
        {
            _auditMonitoringDAL = auditMonitoringDAL;
        }
        
        public async Task<string> SubmitToBranch(SubmitBranchRequest request)
        {
            return await _auditMonitoringDAL.SubmitToBranch(request);
        }
       
        public async Task<string> Reject(RejectRequest request)
        {
            return await _auditMonitoringDAL.Reject(request);
        }

        public async Task<byte[]> Download(DownloadRequest request)
        {
            return await _auditMonitoringDAL.Download(request);
        }
    }

    
}
