namespace CallAuditPortal1.Model.RequestDTO
{
    public class AuditClaimRequest
    {

    }

  
public class SaveStatusRequest
    {
        public string SessionId { get; set; }

        public int AuditTypeId { get; set; }

        public string Status { get; set; }

        public List<string> SelectedIds { get; set; }
    }



    public class RejectUploadedDataRequest 
    { 
        public string SessionId { get; set; } 
        public int AuditTypeId { get; set; } 
        public string Status { get; set; } 
        public string Reason { get; set; } 
        public List<string> SelectedIds { get; set; } }
}

