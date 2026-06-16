using CallAuditPortal1.Service.DAL;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Data;

namespace CallAuditPortal1.Service.BAL
{
  public class AuditBAL
  {
    private readonly AuditDAL _auditDAL;
        private readonly IWebHostEnvironment _webHostEnvironment;
    public AuditBAL(AuditDAL auditDAL, IWebHostEnvironment webHostEnvironment)
        {
            _auditDAL = auditDAL;
            _webHostEnvironment = webHostEnvironment;
        }

        public List<Dictionary<string, object>> GetDropdown()
    {
      try
      {
        return _auditDAL.ReadAuditDropList();
      }
      catch (Exception)
      {

        throw;
      }
    }

    public async Task<(byte[], string)> DownloadTemplate(int auditType)
    {
      try
      {
        string file_path = await _auditDAL.DownloadTemplate(auditType);
                string physicalPath = Path.Combine(
    _webHostEnvironment.WebRootPath,
    file_path.TrimStart('/')
);
                if (string.IsNullOrWhiteSpace(file_path) || !File.Exists(physicalPath))
                {
                    return (null, "");
                }
                string fileName = Path.GetFileName(physicalPath);
                return (System.IO.File.ReadAllBytes(physicalPath), fileName);
            }
      catch (Exception)
      {
        throw;
      }
    }
  }
}
