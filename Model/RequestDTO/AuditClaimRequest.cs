namespace CallAuditPortal1.Model.RequestDTO
{
    public class AuditUploadClaimRequest
    {
        public IFormFile File { get; set; }
        public string AuditTypeId { get; set; }
        public string FromDate { get; set; }
    }
  
public class SaveStatusRequest
    {
        public string SessionId { get; set; }
        public int AuditTypeId { get; set; }
        public string Status { get; set; }
        public List<string> SelectedIds { get; set; }
    }
    public class UpdateUploadedDataRequest
    {
        public string SessionId { get; set; }
        public int AuditTypeId { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public List<string> SelectedIds { get; set; }
    }


    public class RejectUploadedDataRequest 
    { 
        public string SessionId { get; set; } 
        public int AuditTypeId { get; set; } 
        public string Status { get; set; } 
        public string Reason { get; set; } 
        public List<string> SelectedIds { get; set; }
    }
}

