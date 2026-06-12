using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IDataLoaderDAL
    {

        Task<string> UploadData(string sessionId, string templateId, string auditDate, string userName);
        Task<(int TotalData, List<dynamic>)> SearchAuditData(AuditSearchRequest request);
        Task<string> UpdateStatus(UpdateUploadedDataRequest request);
    }
}
