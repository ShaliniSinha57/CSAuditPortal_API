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

        public string Status { get; set; }

        public string AuditType { get; set; }

        public List<int> SelectedIds { get; set; }
    }


    public class Email
    {
        public string To { get; set; }

        public string CC { get; set; }

        public string From { get; set; }

        public string MailSubject { get; set; }

        public string MailBody { get; set; }

        public string AttachmentFileName { get; set; }

        public List<string> Attachments { get; set; } = new();
    }

    public class RejectRequest
    {
        public string Reason { get; set; } = string.Empty;
        public List<int> Ids { get; set; }
    }
}

