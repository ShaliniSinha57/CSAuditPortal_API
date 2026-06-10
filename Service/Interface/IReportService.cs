using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Model.ResponseDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IReportService
    {
        Task<List<BranchResponse>> GetBranches();
        Task<List<FeedbackStatusResponse>> GetFeedbackStatusReport(FeedbackStatusRequest request);
      Task<byte[]> ExportFeedbackStatusReport(FeedbackStatusRequest request);
      Task<List<SummaryStatusResponse>> GetSummaryStatusReport(SummaryStatusRequest request);
      Task<byte[]> ExportSummaryStatusReport(SummaryStatusRequest request);


    }
    
}
