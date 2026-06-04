namespace CallAuditPortal1.Model.ResponseDTO
{
    public class UploadResponse
    {
        public string Status { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Error { get; set; }
        public string Message {  get; set; }
    }
}
