namespace CallAuditPortal1.Model.RequestDTO
{
    
    public class AuditSearchRequest
    {
        public string? SessionId { get; set; }
        public int? AuditTypeId { get; set; }
        public string? AuditStatus { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public int? Page { get; set; }
        public int? Limit { get; set; }
    }

    public class SubmitBranchRequest
    {
        public int Id { get; set; }

        public string ClaimNo { get; set; }

        public string AuditType { get; set; }

        public string Email { get; set; }
    }


    public class DownloadRequest
    {
        public List<int> Ids { get; set; }
        public int AuditId { get; set; }
    }

    public class RejectRequest
    {
        public string Reason { get; set; } = string.Empty;
        public List<int> Ids { get; set; }
    }
}

