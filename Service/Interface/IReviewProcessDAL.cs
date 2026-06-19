using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IReviewProcessDAL
    {
        Task<(int, List<dynamic>)> SearchReviewProcess(ReviewProcessSearchRequest request);
        Task<byte[]> DownloadReviewProcess(ReviewProcessSearchRequest request);
    }
}
