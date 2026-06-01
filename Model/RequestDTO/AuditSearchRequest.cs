namespace CallAuditPortal1.Model.RequestDTO
{
    
        public class AuditSearchRequest
        {
            public string Status { get; set; }

            public string AuditType { get; set; }

            public string FromDate { get; set; }

            public string ToDate { get; set; }
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
        }

    public class RejectRequest
    {
        public string Reason { get; set; } = string.Empty;
        public List<int> Ids { get; set; }
    }
}

