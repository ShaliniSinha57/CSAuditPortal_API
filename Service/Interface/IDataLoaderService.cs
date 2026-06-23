using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IDataLoaderService
    {
            Task<(string result, string session_Id, bool status)> InsertDataIntoTempTable(AuditUploadClaimRequest request);

            Task<(int TotalData, List<dynamic>)> SearchAuditData(AuditSearchRequest request);

            Task<string> SaveStatus(SaveStatusRequest request);

            Task<string> RejectStatus(RejectUploadedDataRequest request);
    }

    }


