using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Service.Interface;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Diagnostics;

namespace CallAuditPortal1.Service
{
    public class DataLoaderService : IDataLoaderService
    {
        private readonly IConfiguration _configuration;
        private readonly IDataLoaderDAL _dataLoaderDAL;
        public DataLoaderService(IConfiguration configuration, IDataLoaderDAL dataLoaderDAL)
        {
            _configuration = configuration;
            _dataLoaderDAL = dataLoaderDAL;
        }
        public async Task<(string result, string session_Id)> InsertDataIntoTempTable(AuditUploadClaimRequest request)
        {
            try
            {
                string sessionId = "";
                using (OracleConnection con = new OracleConnection(
                    _configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();
                    string query = "SELECT USERENV('SESSIONID') FROM DUAL";
                    using (OracleCommand cmd = new OracleCommand(query, con))
                    {
                        sessionId = Convert.ToString(await cmd.ExecuteScalarAsync());
                        string sessionid = sessionId;
                    }

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var stream = request.File.OpenReadStream();
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                        {
                            return ("Worksheet not found.", "");
                        }
                        if (worksheet.Dimension == null)
                        {
                            return ("Excel sheet is empty.", "");
                        }
                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = Math.Min(worksheet.Dimension.Columns, 148);
                        DataTable dt = new DataTable();

                        dt.Columns.Add("ATTRIBUTE1");
                        dt.Columns.Add("ATTRIBUTE2");
                        dt.Columns.Add("CREATION_DATE", typeof(DateTime));
                        dt.Columns.Add("CREATED_BY");

                        for (int i = 3; i <= 150; i++)
                        {
                            dt.Columns.Add($"ATTRIBUTE{i}");
                        }
                        for (int row = 2; row <= rowCount; row++)
                        {
                            DataRow dr = dt.NewRow();

                            dr["ATTRIBUTE1"] = sessionId;
                            dr["ATTRIBUTE2"] = request.AuditTypeId;
                            dr["CREATION_DATE"] = DateTime.Now;
                            dr["CREATED_BY"] = "SYSTEM_USER";

                            int attributeIndex = 3;

                            for (int col = 1; col <= colCount; col++)
                            {

                                if (attributeIndex > 150)
                                {
                                    break;
                                }
                                string value = worksheet.Cells[row, col].Text?.Trim();

                                dr[$"ATTRIBUTE{attributeIndex}"] =
                                    string.IsNullOrWhiteSpace(value)
                                        ? DBNull.Value
                                        : (object)value;

                                attributeIndex++;
                            }
                            dt.Rows.Add(dr);
                           
                        }
                        using(OracleTransaction trans = con.BeginTransaction())
                        {
                            try
                            {
                                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(con))
                                {
                                    bulkCopy.DestinationTableName = "CSNET_PLUS_INTERFACE_ALL";

                                    bulkCopy.BatchSize = 1000;
                                    bulkCopy.BulkCopyTimeout = 0;
                                    bulkCopy.ColumnMappings.Add("ATTRIBUTE1", "ATTRIBUTE1");
                                    bulkCopy.ColumnMappings.Add("ATTRIBUTE2", "ATTRIBUTE2");
                                    bulkCopy.ColumnMappings.Add("CREATION_DATE", "CREATION_DATE");
                                    bulkCopy.ColumnMappings.Add("CREATED_BY", "CREATED_BY");

                                    for (int i = 3; i <= 150; i++)
                                    {
                                        bulkCopy.ColumnMappings.Add(
                                            $"ATTRIBUTE{i}",
                                            $"ATTRIBUTE{i}");
                                    }

                                    bulkCopy.WriteToServer(dt);
                                }
                                trans.Commit();
                            }
                            catch
                            {
                                trans.Rollback();
                                throw;
                            }
                        }
                        
                    }

                    var uploadProcess = await _dataLoaderDAL.UploadData(sessionId, request.AuditTypeId, request.FromDate, "System_user");

                    //return
                    //    $"Data inserted successfully. Session ID : {sessionId}";
                    if (!string.IsNullOrEmpty(uploadProcess))
                    {
                        return (uploadProcess, sessionId);
                    }
                    else
                    {
                        return
                            ($"Data inserted successfully. Session ID : {sessionId}", "");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<(int TotalData, List<dynamic>)> SearchAuditData(AuditSearchRequest request)
        {
            return await _dataLoaderDAL.SearchAuditData(request);
        }
        public async Task<string> SaveStatus(SaveStatusRequest request)
        {
            return await _dataLoaderDAL.UpdateStatus(new UpdateUploadedDataRequest
            {
                SessionId = request.SessionId,
                AuditTypeId = request.AuditTypeId,
                Status = request.Status,
                SelectedIds = request.SelectedIds
            });
        }
        public async Task<string> RejectStatus(RejectUploadedDataRequest request)
        {
            return await _dataLoaderDAL.UpdateStatus(new UpdateUploadedDataRequest
            {
                SessionId = request.SessionId,
                AuditTypeId = request.AuditTypeId,
                Status = request.Status,
                Reason = request.Reason,
                SelectedIds = request.SelectedIds
            });
        }
        //public string DownloadTemplate(int auditTypeId)
        //{
        //    try
        //    {
        //        return _dataLoaderDAL.DownloadTemplate(auditTypeId);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        Task<string> IDataLoaderService.DownloadTemplate(int auditTypeId)
        {
            throw new NotImplementedException();
        }
    }

}
