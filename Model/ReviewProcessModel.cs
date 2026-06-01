namespace CallAuditPortal1.Model
{
    public class ReviewProcessModel
    {
        public string GSFSReceiptNo { get; set; }

        public string DateMonth { get; set; }

        public string FeedbackStatus { get; set; } // Feedback / No Feedback

        public int AgingDays { get; set; }

        public DateTime? CustomerFeedbackDate { get; set; }

        public DateTime? AuditDate { get; set; }

        public string AuditType { get; set; }

        public DateTime? ESCLGCDate { get; set; }

        public string SaleName { get; set; }
    }
}
