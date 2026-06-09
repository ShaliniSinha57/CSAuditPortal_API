namespace CallAuditPortal1.Model.RequestDTO
{
    public class ReviewProcessSearchRequest
    {
        public string ReceiptNo { get; set; }
        public string Suspicious { get; set; }
        public DateTime? FromAuditDate { get; set; }
        public DateTime? ToAuditDate { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
    public class ReviewProcessSearchResponse
    {
        public int Count { get; set; }

        public List<dynamic> Data { get; set; } = new();
    }
    public class DownloadReviewProcessRequest
    {
        public List<string> SelectedIds { get; set; }
    }
}
