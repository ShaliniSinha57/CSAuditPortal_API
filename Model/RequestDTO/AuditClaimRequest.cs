namespace CallAuditPortal1.Model.RequestDTO
{
    public class AuditClaimRequest
    {

    }

    public class SaveStatusRequest
    {
        public string Status { get; set; }
        public List<int> SelectedIds { get; set; }
    }

    public class RejectUploadedDataRequest
    {
        public string Reason { get; set; }
        public List<int> SelectedIds { get; set; }
    }
}
