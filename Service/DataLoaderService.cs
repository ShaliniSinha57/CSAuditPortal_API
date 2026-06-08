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

        public DataLoaderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> UploadData(string sessionId, string templateId, string auditDate, string userName)
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (OracleConnection con =
                    new OracleConnection(connectionString))
                {
                    using (OracleCommand cmd =
                        new OracleCommand("excel_pkg.excel_upld_proc", con))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        DateTime parsedDate;
                        if (string.IsNullOrWhiteSpace(auditDate))
                        {
                            return "Audit date is required.";
                        }

                        cmd.Parameters.Add("p_session", OracleDbType.Varchar2).Value = sessionId;
                        cmd.Parameters.Add("p_template_id", OracleDbType.Int32).Value = Convert.ToInt32(templateId);
                        cmd.Parameters.Add("p_audit_date", OracleDbType.Varchar2).Value = auditDate.Trim();
                        cmd.Parameters.Add("p_user", OracleDbType.Varchar2).Value = userName;
                        OracleParameter statusParam = new OracleParameter("p_status", OracleDbType.Varchar2, 100);
                        statusParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(statusParam);
                        OracleParameter insertedParam = new OracleParameter("p_inserted", OracleDbType.Int32);
                        insertedParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(insertedParam);
                        OracleParameter updatedParam = new OracleParameter("p_updated", OracleDbType.Int32);
                        updatedParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(updatedParam);
                        OracleParameter errorParam = new OracleParameter("p_errors", OracleDbType.Int32);
                        errorParam.Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(errorParam);
                        await con.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                        string status = statusParam.Value?.ToString();
                        int inserted = Convert.ToInt32(insertedParam.Value.ToString());
                        int updated = Convert.ToInt32(updatedParam.Value?.ToString());
                        int errors = Convert.ToInt32(errorParam.Value?.ToString());
                        var response = new UploadResponse
                        {
                            Status = status,
                            Inserted = inserted,
                            Updated = updated,
                            Error = errors,
                            Message = errors > 0 ? "File Uploaded with error" : "File Uploaded Successfully"

                        };
                        return
                            $"Status : {status}\n" +
                            $"Inserted : {inserted}\n" +
                            $"Updated : {updated}\n" +
                            $"Errors : {errors}";
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<(string result, string session_Id)> InsertDataIntoTempTable(string fullPath, string auditType, string auditDate)
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
                    using (var package = new ExcelPackage(new FileInfo(fullPath)))
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
                                insertCmd.Parameters.Add("TEMPLATE_ID", OracleDbType.Varchar2).Value = auditType;
                                columns.Add("CREATION_DATE");
                                values.Add("SYSDATE");
                                columns.Add("CREATED_BY");
                                values.Add(":CREATED_BY");
                                insertCmd.Parameters.Add("CREATED_BY", OracleDbType.Varchar2).Value = "SYSTEM_USER";
                                columns.Add("ATTRIBUTE104");
                                values.Add(":AUDIT_DATE");

                                DateTime parsedAuditDate = DateTime.Parse(auditDate);

                                insertCmd.Parameters.Add("AUDIT_DATE", OracleDbType.Varchar2)
                                         .Value = auditDate;
                                int attributeIndex = 3;

                                for (int col = 1; col <= colCount; col++)
                                {
                                    // IMPORTANT
                                    // Skip ATTRIBUTE104 because it already contains AUDIT_DATE

                                    if (attributeIndex == 104)
                                    {
                                        attributeIndex++;
                                    }

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
                    var uploadProcess = await UploadData(sessionId, auditType, auditDate, "System_user");
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
            catch (Exception ex)
            {
                return ($"Error while inserting data : {ex.Message}", "");
            }
        }
        public async Task<(int TotalData, List<dynamic>)> SearchAuditData(AuditSearchRequest request)
        {
            List<dynamic> data = new List<dynamic>();

            try
            {
                using (OracleConnection con =
                       new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();

                    var screenType = string.IsNullOrWhiteSpace(request.SessionId)
                 ? "SUBMIT"
                 : "VERIFY";
                    using (OracleCommand cmd =
                           new OracleCommand("report_pkg.get_audit_data", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("p_session_id", OracleDbType.Varchar2)
                           .Value = request.SessionId;

                        cmd.Parameters.Add("p_audit_status", OracleDbType.Varchar2)
                           .Value = request.AuditStatus;

                        cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32)
                           .Value = request.AuditTypeId;
                       
                        cmd.Parameters.Add("p_screen_type", OracleDbType.Varchar2)
                           .Value = screenType;

                        cmd.Parameters.Add("p_from_date", OracleDbType.Varchar2)
                           .Value = request.FromDate;

                        cmd.Parameters.Add("p_to_date", OracleDbType.Varchar2)
                           .Value = request.ToDate;

                        cmd.Parameters.Add("p_pageIndex", OracleDbType.Int32)
                           .Value = request.Page != null ? request.Page-1 : request.Page;

                        cmd.Parameters.Add("p_pageSize", OracleDbType.Int32)
                           .Value = request.Limit;

                        cmd.Parameters.Add("p_err", OracleDbType.Varchar2, 4000)
                           .Direction = ParameterDirection.Output;

                        cmd.Parameters.Add("p_count", OracleDbType.Int32)
                           .Direction = ParameterDirection.Output;

                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor)
                           .Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();

                        OracleRefCursor refCursor = (OracleRefCursor)cmd.Parameters["p_result"].Value;
                       
                        using (OracleDataReader reader = refCursor.GetDataReader())
                        {
                            while (await reader.ReadAsync())
                            {
                                Console.WriteLine("Row Found");
                                var row = new ExpandoObject() as IDictionary<string, object>;

                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row.Add(
                                        reader.GetName(i),
                                        reader.IsDBNull(i)
                                            ? null
                                            : reader.GetValue(i)
                                    );
                                }

                                data.Add(row);
                            }
                        }
                        string errMsg = cmd.Parameters["p_err"].Value?.ToString();
                        int count = Convert.ToInt32(cmd.Parameters["p_count"].Value?.ToString());
                        return (count, data);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error in VerifyUpload : " + ex.Message +
                    " | Inner Exception : " + ex.InnerException?.Message
                );
            }
        }
        public async Task<string> SaveStatus(SaveStatusRequest request)
        {
            using (OracleConnection con = new OracleConnection(
                   _configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();

                using (OracleCommand cmd =
                       new OracleCommand("csnet_plus_excel_pkg.verify_reject_uploaded_data", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    string receiptNos = string.Join(",", request.SelectedIds);
                    cmd.Parameters.Add("p_session_id",OracleDbType.Varchar2).Value = request.SessionId;
                    cmd.Parameters.Add("p_audit_type_id",OracleDbType.Int32).Value = request.AuditTypeId;
                    cmd.Parameters.Add("p_remarks", OracleDbType.Varchar2).Value = null;
                    cmd.Parameters.Add("p_status",OracleDbType.Varchar2).Value = "PENDING";
                    cmd.Parameters.Add("p_gsfs_receipt_nos",OracleDbType.Varchar2).Value = receiptNos;
                    cmd.Parameters.Add("p_msg",OracleDbType.Varchar2,4000).Direction = ParameterDirection.Output;
                    await cmd.ExecuteNonQueryAsync();
                    return cmd.Parameters["p_msg"].Value?.ToString();
                }
            }
        }
        public async Task<string> RejectStatus(RejectUploadedDataRequest request)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return "Please provide reason to reject.";
                }

                using (OracleConnection con = new OracleConnection(
                       _configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();

                    using (OracleCommand cmd =
                           new OracleCommand("excel_pkg.verify_reject_uploaded_data", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Convert List to comma separated string
                        string receiptNos = string.Join(",", request.SelectedIds);

                        // INPUT PARAMETERS
                        cmd.Parameters.Add(
                            "p_session_id",
                            OracleDbType.Varchar2
                        ).Value = request.SessionId;

                        cmd.Parameters.Add(
                            "p_audit_type_id",
                            OracleDbType.Int32
                        ).Value = request.AuditTypeId;

                        cmd.Parameters.Add(
                            "p_remarks",
                            OracleDbType.Varchar2
                        ).Value = request.Reason;

                        cmd.Parameters.Add(
                            "p_status",
                            OracleDbType.Varchar2
                        ).Value = "REJECTED";

                        cmd.Parameters.Add(
                            "p_gsfs_receipt_nos",
                            OracleDbType.Varchar2
                        ).Value = receiptNos;

                        // OUTPUT PARAMETER
                        cmd.Parameters.Add(
                            "p_msg",
                            OracleDbType.Varchar2,
                            4000
                        ).Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();

                        return cmd.Parameters["p_msg"].Value?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

}
