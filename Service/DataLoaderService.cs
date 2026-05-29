using CallAuditPortal1.Service.Interface;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Security.Cryptography;

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
                        new OracleCommand("csnet_plus_excel_pkg.csnet_plus_excel_upld_proc", con))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;
                        DateTime parsedDate;
                        if (!DateTime.TryParse(auditDate, out parsedDate))
                        {
                            return "Invalid audit date format.";
                        }
                        cmd.Parameters.Add("p_session", OracleDbType.Varchar2).Value = sessionId;
                        cmd.Parameters.Add("p_template_id", OracleDbType.Int32).Value = Convert.ToInt32(templateId);
                        cmd.Parameters.Add("p_audit_date", OracleDbType.Date).Value = parsedDate;
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
                        string inserted = insertedParam.Value?.ToString();
                        string updated = updatedParam.Value?.ToString();
                        string errors = errorParam.Value?.ToString();
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

        public async Task<string> InsertDataIntoTempTable(string fullPath, string auditType, string auditDate)
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

                    }

                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(new FileInfo(fullPath)))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                        {
                            return "Worksheet not found.";
                        }
                        if (worksheet.Dimension == null)
                        {
                            return "Excel sheet is empty.";
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
                                insertCmd.Parameters.Add("AUDIT_DATE", OracleDbType.Date).Value = parsedAuditDate;
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
                        return uploadProcess;
                    }
                    else
                    {
                        return
                            $"Data inserted successfully. Session ID : {sessionId}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error while inserting data : {ex.Message}";
            }
        }

        public async Task<List<dynamic>> VerifyUpload(string sessionId,string templateId)
        {
            List<dynamic> data = new List<dynamic>();

            using (OracleConnection con = new OracleConnection(
                    _configuration.GetConnectionString("DefaultConnection")))
            {
                await con.OpenAsync();

                string query = @"";

                using (OracleCommand cmd =
                       new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("sessionId",
                        OracleDbType.Varchar2).Value = sessionId;

                    cmd.Parameters.Add("templateId",
                        OracleDbType.Varchar2).Value = templateId;

                    using (OracleDataReader reader =
                           await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var row = new ExpandoObject()
                                      as IDictionary<string, object>;

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row.Add(
                                    reader.GetName(i),
                                    reader.IsDBNull(i)
                                    ? null
                                    : reader.GetValue(i));
                            }

                            data.Add(row);
                        }
                    }
                }
            }

            return data;
        }




    }
}