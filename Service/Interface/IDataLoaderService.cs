using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.DAL;

namespace CallAuditPortal1.Service.Interface
{
    public interface IDataLoaderService
    {

        Task<(string result, string session_Id)> InsertDataIntoTempTable(string fullPath, string auditType, string auditDate);

        Task<string> UploadData(string sessionId, string templateId, string auditDate, string userName);
        Task<List<dynamic>> VerifyUpload(string sessionId, string templateId);

        Task<string> SaveStatus(SaveStatusRequest request);
        Task<string> RejectStatus(RejectUploadedDataRequest request);

    }

}
