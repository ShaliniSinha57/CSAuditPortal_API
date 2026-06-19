using CallAuditPortal1.Service.Interface;

namespace CallAuditPortal1.Service.BAL
{
    public class FileUploadBAL : IFileUploadBAL
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUploadBAL(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public Task<bool> DeleteFile(string filePath)
        {
            string physicalPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                filePath.TrimStart('/')
            );

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            return Task.FromResult(true);
        }

        public async Task<string> SaveFile(IFormFile file)
        {
            if (file == null && string.IsNullOrWhiteSpace(file.FileName))
                return string.Empty;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "UploadedAttachment");

            if(!Path.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string fileName = $"Attachment_{Guid.NewGuid()+Path.GetExtension(file.FileName)}";

            string filePath = Path.Combine(uploadsFolder, fileName);
            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/UploadedAttachment/{fileName}";
        }
    }
}
