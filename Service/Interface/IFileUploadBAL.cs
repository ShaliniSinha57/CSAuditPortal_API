namespace CallAuditPortal1.Service.Interface
{
    public interface IFileUploadBAL
    {
        Task<string> SaveFile(IFormFile file);
        Task<bool> DeleteFile(string filePath);
    }
}
