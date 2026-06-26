namespace CallAuditPortal1.Model.RequestDTO
{
    public class MailDynamicRow
    {
        public string AuditHead { get; set; } = string.Empty;

        public int AcceptedCount { get; set; }

        public int RejectedCount { get; set; }

        public string EscName { get; set; } = string.Empty;

        public int FeedbackSubmitCount { get; set; }

        public int NewUpload { get; set; }

        public int PreviousPending { get; set; }

        public int Total { get; set; }

    }
   
}
