using static CallAuditPortal1.Model.RequestDTO.EvaluationProcessRequest;

namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditEvaluationProcessDAL
    {
        Task<dynamic> Get_Evaluation_Data(string receipt_no, int audit_typeId);

        Task<string> SaveFeedbackStatus(SaveFeedbackRequest request);
    }
}
