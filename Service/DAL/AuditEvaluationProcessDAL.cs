using CallAuditPortal1.Service.Interface;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Dynamic;
using static CallAuditPortal1.Model.RequestDTO.EvaluationProcessRequest;

namespace CallAuditPortal1.Service.DAL
{
    public class AuditEvaluationProcessDAL : IAuditEvaluationProcessDAL
    {
        private readonly IConfiguration _configuration;

        public AuditEvaluationProcessDAL(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<dynamic> Get_Evaluation_Data(string receipt_no, int audit_typeId)
        {
            try
            {
               dynamic data = null;

                using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand("CSNET_PLUS.CSNET_PLUS_REPORT_PKG.get_evaluation_process", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.BindByName = true;
                        cmd.Parameters.Add("p_gsfs_receipt_no", OracleDbType.Varchar2).Value = receipt_no;
                        cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32).Value = audit_typeId;

                        cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();
                        string errMsg = cmd.Parameters["p_msg"].Value?.ToString();

                        if (!string.IsNullOrEmpty(errMsg) && errMsg != "SUCCESS")
                        {
                            throw new Exception(errMsg);
                        }

                        if (cmd.Parameters["p_result"].Value == DBNull.Value)
                        {
                            throw new Exception("No cursor returned from procedure.");
                        }
                        OracleRefCursor refCursor = (OracleRefCursor)cmd.Parameters["p_result"].Value;

                        using (OracleDataReader reader = refCursor.GetDataReader())
                        {
                            while(await reader.ReadAsync())
                            {
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

                                data = row;
                            }
                        }

                        errMsg = cmd.Parameters["p_msg"].Value?.ToString();

                        return data;
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> SaveFeedbackStatus(SaveFeedbackRequest request)
        {
            try
            {
                using(OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();
                    using(OracleCommand cmd = new OracleCommand("CSNET_PLUS_REPORT_PKG.save_feedback", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_gsfs_receipt_no", OracleDbType.Varchar2).Value = request.GSFS_ReceiptNo;
                        cmd.Parameters.Add("p_audit_type_id", OracleDbType.Varchar2).Value = request.AuditTypeId;
                        cmd.Parameters.Add("p_attachement_name", OracleDbType.Varchar2).Value = request.AttachementUrl;
                        cmd.Parameters.Add("p_status", OracleDbType.Varchar2).Value = request.Status;
                        cmd.Parameters.Add("p_remarks", OracleDbType.Varchar2).Value = request.Remark;
                        cmd.Parameters.Add("p_f_by", OracleDbType.Varchar2).Value = request.ActionBy;
                        cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 4000).Direction = ParameterDirection.Output;
                        await cmd.ExecuteNonQueryAsync();
                        return cmd.Parameters["p_msg"].Value.ToString();
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
