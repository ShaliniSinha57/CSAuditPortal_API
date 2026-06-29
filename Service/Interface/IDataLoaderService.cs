using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Model.ResponseDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IDataLoaderService
    {
        Task<UploadResponse> InsertDataIntoTempTable(AuditUploadClaimRequest request);
        Task<(byte[] bytes, string fileName)> DownloadErrorRows(int templateId, int sessionId);
        Task<(int TotalData, List<dynamic>)> SearchAuditData(AuditSearchRequest request);

        Task<string> SaveStatus(SaveStatusRequest request);

        Task<string> RejectStatus(RejectUploadedDataRequest request);
    }
}


