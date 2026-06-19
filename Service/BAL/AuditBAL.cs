using CallAuditPortal1.Service.DAL;
using CallAuditPortal1.Service.Interface;
using ClosedXML.Excel;
using System.Data;

namespace CallAuditPortal1.Service.BAL
{
  public class AuditBAL
  {
        private readonly AuditDAL _auditDAL;
        private readonly IDataLoaderDAL _dataLoader;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AuditBAL(AuditDAL auditDAL, IWebHostEnvironment webHostEnvironment, IDataLoaderDAL dataloader)
        {
            _auditDAL = auditDAL;
            _webHostEnvironment = webHostEnvironment;
            _dataLoader = dataloader;
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
                var data = await _dataLoader.FetchTemplateColumnsAsync(auditType);
                var orderedData = data.OrderBy(d => d.EXCEL_COLUMN_NO).ToList();
                string templateName = orderedData.FirstOrDefault().STG_TABLE;
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(templateName);
                    int colIndex = 1;

                    foreach (var item in orderedData)
                    {
                        worksheet.Cell(1, colIndex).Value = item.EXCEL_COLUMN_NAME;
                        worksheet.Cell(1, colIndex).Style.Font.Bold = true;
                        colIndex++;
                    }

                    worksheet.Columns().AdjustToContents();
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return (stream.ToArray(), templateName);
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
  }
}
