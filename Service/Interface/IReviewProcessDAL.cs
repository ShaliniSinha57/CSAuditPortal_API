using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IReviewProcessDAL
    {
        //Task<List<ReviewProcessModel>> SearchReviewProcess(ReviewProcessSearchRequest request);
        Task<byte[]> DownloadReviewProcess(DownloadReviewProcessRequest request);
        Task<ReviewProcessSearchResponse> SearchReviewProcess(ReviewProcessSearchRequest request);
    }
}
