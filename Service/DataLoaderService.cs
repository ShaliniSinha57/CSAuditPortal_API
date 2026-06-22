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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                string sessionId = "";
                var tableColumns = await _dataLoaderDAL.FetchTemplateColumnsAsync(Convert.ToInt32(request.AuditTypeId));
                if (tableColumns == null || tableColumns.Count == 0)
                {
                    return ("Invalid audit type.", "");
                }
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

                        dt.Columns.Add("FILETYPE_ID");
                        dt.Columns.Add("FILE_ID");
                        dt.Columns.Add("PROCESS_FLAG");
                        dt.Columns.Add("AUDIT_DATE");
                        foreach(var item in tableColumns)
                        {
                            dt.Columns.Add(item.DB_COLUMN);
                        }

                        Dictionary<string, int> excelHeaderMap = new Dictionary<string, int>();
                        
                        for (int col = 1; col <= colCount; col++)
                        {
                            string header = worksheet.Cells[1, col].Text?.Trim();

                            if (!string.IsNullOrWhiteSpace(header))
                            {
                                excelHeaderMap[header.ToUpper()] = col;
                            }
                        }



                        for (int row = 2; row <= rowCount; row++)
                        {
                            DataRow dr = dt.NewRow();

                            dr["FILETYPE_ID"] = request.AuditTypeId;
                            dr["FILE_ID"] = sessionId;
                            dr["PROCESS_FLAG"] = "N";
                            dr["AUDIT_DATE"] = request.FromDate;

                            foreach(var item in tableColumns)
                            {
                                if(excelHeaderMap.TryGetValue(item.EXCEL_COLUMN_NAME.ToUpper(), out int colNo))
                                {
                                    string value = worksheet.Cells[row, colNo].Text?.Trim();

                                    dr[item.DB_COLUMN] = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
                                }
                                else
                                {
                                    dr[item.DB_COLUMN] = DBNull.Value;
                                }
                            }

                            dt.Rows.Add(dr);
                           
                        }
                        using(OracleTransaction trans = con.BeginTransaction())
                        {
                            try
                            {
                                using (OracleBulkCopy bulkCopy = new OracleBulkCopy(con))
                                {
                                    bulkCopy.DestinationTableName = tableColumns.FirstOrDefault().STG_TABLE;

                                    bulkCopy.BatchSize = 1000;
                                    bulkCopy.BulkCopyTimeout = 0;
                                    bulkCopy.ColumnMappings.Add("FILETYPE_ID", "FILETYPE_ID");
                                    bulkCopy.ColumnMappings.Add("FILE_ID", "FILE_ID");
                                    bulkCopy.ColumnMappings.Add("PROCESS_FLAG", "PROCESS_FLAG");
                                    bulkCopy.ColumnMappings.Add("AUDIT_DATE", "AUDIT_DATE");

                                    foreach(var item in tableColumns)
                                    {
                                        bulkCopy.ColumnMappings.Add(item.DB_COLUMN, item.DB_COLUMN);
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

                    sw.Stop();

                    Console.WriteLine("staging table insertion time", sw.Elapsed());

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

    
    }

}
