using CallAuditPortal1.Service.DAL;
using System.Data;

namespace CallAuditPortal1.Service.BAL
{
  public class AuditBAL
  {
    private readonly AuditDAL _auditDAL;
    public AuditBAL(AuditDAL auditDAL)
    {
          _auditDAL = auditDAL;
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

    public List<Dictionary<string, object>> DownloadTemplate()
    {
      try
      {
        return _auditDAL.DownloadTemplate();
      }
      catch (Exception)
      {
        throw;
      }
    }
  }
}
