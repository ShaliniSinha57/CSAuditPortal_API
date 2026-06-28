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
        public int AuditTypeId { get; set; }
        //public List<string> GSFS_Receipt_Nos { get; set; }
        public List<string> GSFS_Receipt_Nos { get; set; } = new();
    }

    public class Email
    {
        // Mail Details
        public string To { get; set; } = string.Empty;

        public string CC { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;

        public string MailSubject { get; set; } = string.Empty;

        public string MailBody { get; set; } = string.Empty;

        public string Header { get; set; } = string.Empty;

        public string Footer { get; set; } = string.Empty;

        // Audit Details
        public string Process { get; set; } = string.Empty;
        public string ShipToCode { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string AuditMonth { get; set; } = string.Empty;

        public string AuditYear { get; set; } = string.Empty;

        public string LastDate { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;

        public string SessionId { get; set; } = string.Empty;

        // Attachment
        public string AttachmentFileName { get; set; } = string.Empty;

        public List<string> Attachments { get; set; } = new();

        // Dynamic Table Data
        public List<MailDynamicRow> Rows { get; set; } = new();
    }
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class MailResponse
    {
        public bool Success { get; set; }
        public string SuccessDetails { get; set; }
    }

  
    public class RejectRequest
    {
        public int AuditTypeId { get; set; }
        public List<string> GSFS_Receipt_Nos { get; set; }
    }

    public class DownloadRequest
    {
        public int AuditTypeId { get; set; }
        public List<string> GSFS_Receipt_Nos { get; set; }
    }
}

