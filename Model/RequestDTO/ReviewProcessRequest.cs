namespace CallAuditPortal1.Model.RequestDTO
{
    public class ReviewProcessRequest
    {
        public string ReceiptNo { get; set; }
        public string Suspicious { get; set; }
        public DateTime? FromAuditDate { get; set; }
        public DateTime? ToAuditDate { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    public class DownloadReviewProcessRequest
    {
        public List<int> SelectedIds { get; set; }
    }
}
