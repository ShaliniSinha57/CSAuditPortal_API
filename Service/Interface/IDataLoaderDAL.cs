using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;

namespace CallAuditPortal1.Service.Interface
{
    public interface IDataLoaderDAL
    {

        Task<(string message, bool status, int insertCount, int updatecount, int errorCount)> UploadData(string sessionId, string templateId, string userName);

        Task<List<Template_Columns>> FetchTemplateColumnsAsync(int templateId);

        Task<(int TotalData, List<dynamic>)> SearchAuditData(AuditSearchRequest request);
        Task<string> UpdateStatus(UpdateUploadedDataRequest request);
        //Task<string> DownloadTemplate(int auditTypeId);
    }
}
