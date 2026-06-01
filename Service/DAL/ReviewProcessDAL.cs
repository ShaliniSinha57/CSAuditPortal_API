using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;

namespace CallAuditPortal1.Service.DAL
{
    public class ReviewProcessDAL : IReviewProcessDAL
    {
        public Task<string> DownloadReviewProcess(DownloadReviewProcessRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<List<ReviewProcessModel>> SearchReviewProcess(ReviewProcessRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
