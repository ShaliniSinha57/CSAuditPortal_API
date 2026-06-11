using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Model.ResponseDTO;
using CallAuditPortal1.Service.Interface;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Configuration;
using System.Data;
using System.Dynamic;

namespace CallAuditPortal1.Service.DAL
{
    public class DataLoaderDAL : IDataLoaderDAL
    {
        private readonly IConfiguration _configuration;
        public DataLoaderDAL(IConfiguration configuration)
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
                           new OracleCommand("CSNET_PLUS_REPORT_PKG.get_audit_data", con))
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
                           .Value = request.Page != null ? request.Page - 1 : request.Page;

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

        public async Task<string> UpdateStatus(UpdateUploadedDataRequest request)
        {
            try
            {
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
                        cmd.Parameters.Add("p_session_id",OracleDbType.Varchar2).Value = request.SessionId;
                        cmd.Parameters.Add("p_audit_type_id",OracleDbType.Int32).Value = request.AuditTypeId;
                        cmd.Parameters.Add("p_remarks",OracleDbType.Varchar2).Value = request.Reason;
                        cmd.Parameters.Add("p_status",OracleDbType.Varchar2).Value = string.IsNullOrWhiteSpace(request.Reason) ? "PENDING" : "REJECTED";
                        cmd.Parameters.Add("p_gsfs_receipt_nos",OracleDbType.Varchar2).Value = receiptNos;

                        // OUTPUT PARAMETER
                        cmd.Parameters.Add("p_msg",OracleDbType.Varchar2,4000).Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();

                        return cmd.Parameters["p_msg"].Value?.ToString();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
