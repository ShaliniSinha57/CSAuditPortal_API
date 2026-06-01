using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IReviewProcessDAL
    {
        Task<List<ReviewProcessModel>> SearchReviewProcess(ReviewProcessRequest request);
        Task<string> DownloadReviewProcess(DownloadReviewProcessRequest request);
    }
}
