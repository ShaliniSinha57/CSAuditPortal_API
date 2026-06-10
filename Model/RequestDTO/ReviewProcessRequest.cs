namespace CallAuditPortal1.Model.RequestDTO
{
    public class ReviewProcessSearchRequest
    {
        public string ReceiptNo { get; set; }
        public int AuditTypeId { get; set; }
        public string Suspicious { get; set; }
        public string FromAuditDate { get; set; }
        public string ToAuditDate { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    public class DownloadReviewProcessRequest
    {
        public List<string> SelectedIds { get; set; }
    }
}
