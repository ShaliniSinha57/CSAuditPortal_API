namespace CallAuditPortal1.Model.RequestDTO
{
    public class EvaluationProcessRequest
    {
        public class SaveFeedbackRequest
        {
            public string GSFS_ReceiptNo { get; set; }
            public string AuditTypeId { get; set; }
            //public string AttachmentName { get; set; }
            public string Status { get; set; }
            public string Remark { get; set; }
            public IFormFile? Attachement { get; set; }
            public string? AttachementUrl { get; set; }
            public string ActionBy { get; set; }
        }
    }
}
