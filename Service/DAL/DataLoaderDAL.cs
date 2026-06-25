using CallAuditPortal1.Model;
using CallAuditPortal1.Model.RequestDTO;
using CallAuditPortal1.Model.ResponseDTO;
using CallAuditPortal1.Service.Interface;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.CodeDom;
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

        public async Task<(string message, bool status, int insertCount, int updatecount, int errorCount)> UploadData(string sessionId, string templateId, string userName)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (OracleConnection con = new OracleConnection(connectionString))
                {
                    await con.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand("PKG_TEMPLATE_UPLOAD.PROC_PROCESS_UPLOAD", con))
                    {

                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("P_FILEID", OracleDbType.Varchar2).Value = sessionId;
                        cmd.Parameters.Add("P_FILETYPEID", OracleDbType.Int32).Value = Convert.ToInt32(templateId);

                        //Output
                        cmd.Parameters.Add("P_STATUS_FLAG", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_INS_COUNT", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_UPD_COUNT", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_ERR_COUNT", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_STATUS_MSG", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                        
                        await cmd.ExecuteNonQueryAsync();

                        string message = cmd.Parameters["P_STATUS_MSG"].Value.ToString();
                        int flag = ((OracleDecimal)cmd.Parameters["P_STATUS_FLAG"].Value).ToInt32();
                        int intsertCount = ((OracleDecimal)cmd.Parameters["P_INS_COUNT"].Value).ToInt32();
                        int updateCount = ((OracleDecimal)cmd.Parameters["P_UPD_COUNT"].Value).ToInt32();
                        int errorCount = ((OracleDecimal)cmd.Parameters["P_ERR_COUNT"].Value).ToInt32();

                        
                        return (message, flag == 0, insertCount: intsertCount, updateCount, errorCount);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<Template_Columns>> FetchTemplateColumnsAsync(int templateId)
        {
            try
            {
                List<Template_Columns> columns = new List<Template_Columns>();
                using(OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();
                    using(OracleCommand cmd = new OracleCommand("PKG_TEMPLATE_UPLOAD.PROC_EXCEL_TEMPLATE_INFO", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("P_TEMPLATE_ID", OracleDbType.Int32).Value = templateId;

                        // Output Parameters

                        cmd.Parameters.Add("P_EXC_TEMP", OracleDbType.RefCursor).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_STATUS_FLAG", OracleDbType.Int32).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_STATUS_MSG", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();

                        OracleRefCursor refCurs = (OracleRefCursor)cmd.Parameters["P_EXC_TEMP"].Value;
                        using (OracleDataReader reader = refCurs.GetDataReader())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    columns.Add(new Template_Columns
                                    {
                                        EXCEL_COLUMN_NO = reader["EXCEL_COLUMN_NO"] != DBNull.Value ? Convert.ToInt32(reader["EXCEL_COLUMN_NO"]) : 0,
                                        EXCEL_COLUMN_NAME = reader["EXCEL_COLUMN_NAME"].ToString(),
                                        DB_COLUMN = reader["DB_COLUMN"].ToString(),
                                        STG_TABLE = reader["STG_TABLE"].ToString()
                                    });
                                }
                            }
                        }
                        return columns;
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
                           new OracleCommand("csnet_plus_excel_pkg.verify_reject_uploaded_data", con))
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
