namespace CallAuditPortal1.Model.RequestDTO
{
    public class MailDynamicRow
    {
        public string AuditHead { get; set; } = string.Empty;

        public int AcceptedCount { get; set; }

        public int RejectedCount { get; set; }

        public int FeedbackSubmitCount { get; set; }

        public int NewUpload { get; set; }

        public int PreviousPending { get; set; }

        public int Total { get; set; }
    }

    public class MailResponseModel
    {
        public string ShipToCode { get; set; } = string.Empty;

        public string AuditMonth { get; set; } = string.Empty;

        public string ValidTill { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public string MailTo { get; set; } = string.Empty;

        public string MailCc { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;
        public string RowIds { get; set; } = string.Empty;

        public List<MailDynamicRow> Rows { get; set; } = new();
    }

   

}
