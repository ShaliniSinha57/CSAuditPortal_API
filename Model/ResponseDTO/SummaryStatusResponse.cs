namespace CallAuditPortal1.Model.ResponseDTO
{
    public class SummaryStatusResponse
    {
        public string Branch { get; set; }

        public Dictionary<string, int> TemplateCounts { get; set; } = new();



    }
}
