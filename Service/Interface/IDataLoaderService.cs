using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.DAL;

namespace CallAuditPortal1.Service.Interface
{
    public interface IDataLoaderService
    {
            Task<(string result, string session_Id)> InsertDataIntoTempTable(AuditUploadClaimRequest request);

            Task<(int TotalData, List<dynamic>)> SearchAuditData(AuditSearchRequest request);

            Task<string> SaveStatus(SaveStatusRequest request);

            Task<string> RejectStatus(RejectUploadedDataRequest request);
    }

    }


