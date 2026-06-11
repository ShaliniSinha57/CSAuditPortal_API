using CallAuditPortal1.Service.Interface;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Dynamic;

namespace CallAuditPortal1.Service.DAL
{
    public class AuditEvaluationProcessDAL : IAuditEvaluationProcessDAL
    {
        private readonly IConfiguration _configuration;

        public AuditEvaluationProcessDAL(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<dynamic>> Get_Evaluation_Data(string receipt_no, int audit_typeId)
        {
            try
            {
                List<dynamic> data = new List<dynamic>();

                using (OracleConnection con = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await con.OpenAsync();
                    using (OracleCommand cmd = new OracleCommand("CSNET_PLUS_REPORT_PKG.get_evaluation_process", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("p_gsfs_receipt_no", OracleDbType.Varchar2).Value = receipt_no;
                        cmd.Parameters.Add("p_audit_type_id", OracleDbType.Varchar2).Value = audit_typeId;

                        cmd.Parameters.Add("p_msg", OracleDbType.Varchar2).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("p_result", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        await cmd.ExecuteNonQueryAsync();
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

                                data.Add(row);
                            }
                        }

                        string errMsg = cmd.Parameters["p_msg"].Value?.ToString();

                        return data;
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
