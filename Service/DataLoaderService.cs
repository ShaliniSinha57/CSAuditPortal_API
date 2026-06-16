using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Model.ResponseDTO;
using CallAuditPortal1.Service.Interface;
using DocumentFormat.OpenXml.Office.Word;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Dynamic;

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
                        for (int row = 2; row <= rowCount; row++)
                        {
                            List<string> columns = new List<string>();
                            List<string> values = new List<string>();
                            using (OracleCommand insertCmd = new OracleCommand())
                            {
                                insertCmd.Connection = con;
                                insertCmd.BindByName = true;
                                columns.Add("ATTRIBUTE1");
                                values.Add(":SESSION_ID");
                                insertCmd.Parameters.Add("SESSION_ID", OracleDbType.Varchar2).Value = sessionId;
                                columns.Add("ATTRIBUTE2");
                                values.Add(":TEMPLATE_ID");
                                insertCmd.Parameters.Add("TEMPLATE_ID", OracleDbType.Varchar2).Value = request.AuditTypeId;
                                columns.Add("CREATION_DATE");
                                values.Add("SYSDATE");
                                columns.Add("CREATED_BY");
                                values.Add(":CREATED_BY");
                                insertCmd.Parameters.Add("CREATED_BY", OracleDbType.Varchar2).Value = "SYSTEM_USER";
                                //columns.Add("ATTRIBUTE104");
                                //values.Add(":AUDIT_DATE");

                                //DateTime parsedAuditDate = DateTime.Parse(request.FromDate);

                                //insertCmd.Parameters.Add("AUDIT_DATE", OracleDbType.Varchar2)
                                //         .Value = request.FromDate;
                                int attributeIndex = 3;

                                for (int col = 1; col <= colCount; col++)
                                {
                                    if (attributeIndex > 150)
                                    {
                                        break;
                                    }

                                    string attributeName = $"ATTRIBUTE{attributeIndex}";
                                    string parameterName = $"COL{col}";

                                    columns.Add(attributeName);
                                    values.Add($":{parameterName}");

                                    string cellValue =
                                        worksheet.Cells[row, col].Text?.Trim();

                                    insertCmd.Parameters.Add(
                                        parameterName,
                                        OracleDbType.Varchar2
                                    ).Value =
                                        string.IsNullOrWhiteSpace(cellValue)
                                        ? DBNull.Value
                                        : cellValue;

                                    attributeIndex++;
                                }
                                string insertQuery = $@"INSERT INTO CSNET_PLUS_INTERFACE_ALL
                            (
                                {string.Join(",", columns)}
                            )
                            VALUES
                            (
                                {string.Join(",", values)}
                            )";
                                Console.WriteLine(insertQuery);
                                insertCmd.CommandText = insertQuery;
                                await insertCmd.ExecuteNonQueryAsync();
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
