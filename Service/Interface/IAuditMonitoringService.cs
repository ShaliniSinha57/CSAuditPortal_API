using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using System.Data;

namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditMonitoringService
    {
        Task<string> SubmitToBranch(SubmitBranchRequest request);

        Task<string> Reject(RejectRequest request);


        Task<byte[]> Download(DownloadRequest request);
        Task<bool> SendMailForSucessful(string userid, string role);
        Task<bool> SendMailForAccepted(string userid, string role);
        Task<bool> SendMailForFeedback(string userid, string role);
        Task<bool> SendMailForRejected(string userid, string role);
        
        //Task<List<MailDynamicRow>> GetAcceptedMailRows(string userId);
    }
}
