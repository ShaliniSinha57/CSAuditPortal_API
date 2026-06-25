using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Model.ResponseDTO;
using CallAuditPortal1.Service.Interface;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
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
        public async Task<UploadResponse> InsertDataIntoTempTable(AuditUploadClaimRequest request)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
            {
                string sessionId = "";
                var tableColumns = await _dataLoaderDAL.FetchTemplateColumnsAsync(Convert.ToInt32(request.AuditTypeId));
                if (tableColumns == null || tableColumns.Count == 0)
                {
                    return new UploadResponse
                    {
                        Message = "Invalid audit type."
                    };
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
                            return new UploadResponse
                            {
                                Message = "Worksheet not found."
                            };
                        }
                        if (worksheet.Dimension == null)
                        {
                            return new UploadResponse
                            {
                                Message = "Excel sheet is empty."
                            };
                        }
                        int rowCount = worksheet.Dimension.Rows;
                        int colCount = Math.Min(worksheet.Dimension.Columns, 148);
                        DataTable dt = new DataTable();

                        dt.Columns.Add("TEMPLATE_ID");
                        dt.Columns.Add("SESSION_ID");
                        dt.Columns.Add("PROCESS_FLAG");
                        dt.Columns.Add("AUDIT_DATE");
                        dt.Columns.Add("ROW_NO");
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

                            dr["TEMPLATE_ID"] = request.AuditTypeId;
                            dr["SESSION_ID"] = sessionId;
                            dr["PROCESS_FLAG"] = "N";
                            dr["AUDIT_DATE"] = request.FromDate;
                            dr["ROW_NO"] = row-1;

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
                                    bulkCopy.ColumnMappings.Add("TEMPLATE_ID", "TEMPLATE_ID");
                                    bulkCopy.ColumnMappings.Add("SESSION_ID", "SESSION_ID");
                                    bulkCopy.ColumnMappings.Add("PROCESS_FLAG", "PROCESS_FLAG");
                                    bulkCopy.ColumnMappings.Add("AUDIT_DATE", "AUDIT_DATE");
                                    bulkCopy.ColumnMappings.Add("ROW_NO", "ROW_NO");

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
                    Console.WriteLine($"staging table insertion time: {sw.Elapsed.TotalSeconds} seconds");

                    sw.Start();
                    var uploadProcess = await _dataLoaderDAL.UploadData(sessionId, request.AuditTypeId, "System_user");
                    sw.Stop();
                    Console.WriteLine($"Transfer to main table time : {sw.Elapsed.TotalSeconds} seconds");

                    return new UploadResponse
                    {
                        Message = uploadProcess.message,
                        Status = uploadProcess.status,
                        SessionId = sessionId,
                        Inserted = uploadProcess.insertCount,
                        Updated = uploadProcess.updatecount,
                        Error = uploadProcess.errorCount
                    };
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(byte[] bytes, string fileName)> DownloadErrorRows(int templateId, int sessionId)
        {
            try
            {
                var data = await _dataLoaderDAL.GetErrorData(templateId, sessionId);

                if (data == null || data.Count == 0)
                    return (null, "");

                var excelHeader = await _dataLoaderDAL.FetchTemplateColumnsAsync(templateId);

                var orderedHeaderData = excelHeader.OrderBy(d => d.EXCEL_COLUMN_NO).ToList();
                string templateName = orderedHeaderData.FirstOrDefault().STG_TABLE;

                using (XLWorkbook wb = new XLWorkbook())
                {
                    var worksheet = wb.Worksheets.Add(templateName);
                    int colIndex = 1;

                    foreach (var item in orderedHeaderData)
                    {
                        worksheet.Cell(1, colIndex).Value = item.EXCEL_COLUMN_NAME;
                        worksheet.Cell(1, colIndex).Style.Font.Bold = true;
                        colIndex++;
                    }

                    worksheet.Cell(1, colIndex).Value = "Error";
                    worksheet.Cell(1, colIndex).Style.Font.Bold = true;
                    colIndex++;

                    int rowIndex = 2;

                    foreach (IDictionary<string, object> row in data)
                    {
                        colIndex = 1;

                        foreach (var header in orderedHeaderData)
                        {
                            // Assuming the Oracle cursor column names match EXCEL_COLUMN_NAME
                            if (row.TryGetValue(header.DB_COLUMN, out var value))
                            {
                                worksheet.Cell(rowIndex, colIndex).Value = value?.ToString() ?? "";
                            }
                            else
                            {
                                worksheet.Cell(rowIndex, colIndex).Value = "";
                            }

                            colIndex++;
                        }

                        // Write Error column
                        if (row.TryGetValue("ERR", out var errorValue))
                        {
                            worksheet.Cell(rowIndex, colIndex).Value = errorValue?.ToString() ?? "";
                        }

                        rowIndex++;
                    }

                    worksheet.Columns().AdjustToContents();
                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return (stream.ToArray(), templateName);
                    }
                }

            }
            catch
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
    }

}
