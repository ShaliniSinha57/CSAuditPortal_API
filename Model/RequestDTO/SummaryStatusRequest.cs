namespace CallAuditPortal1.Model.RequestDTO
{
    public class SummaryStatusRequest
    {
        public string? BranchCode { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public string? UserRole { get; set; }

        public string? LoggedInBranch { get; set; }
    }
}
