using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Model.ResponseDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IReportDAL
    {
        Task<List<BranchResponse>> GetBranches();

        Task<(int Count, List<dynamic> Data)> SearchFeedbackStatusReport(
    FeedbackStatusRequest request);

        Task<(int Count, List<dynamic> Data)> SearchSummaryStatusReport(
       SummaryStatusRequest request);
        Task<byte[]> DownloadSummaryStatusReport(ExportSummaryExcel export);
        Task<byte[]> DownloadFeedbackReport(ExportExcel report);

    }
    
}
