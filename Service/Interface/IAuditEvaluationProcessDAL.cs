namespace CallAuditPortal1.Service.Interface
{
    public interface IAuditEvaluationProcessDAL
    {
        Task<List<dynamic>> Get_Evaluation_Data(string receipt_no, int audit_typeId);
    }
}
