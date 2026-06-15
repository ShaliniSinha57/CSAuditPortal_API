using Oracle.ManagedDataAccess.Client;
using System.Data;


namespace CallAuditPortal1.Service.DAL
{
  public class AuditDAL
  {
    private readonly IConfiguration _configuration;

    private readonly Model.DatabaseConnection _connection;


    public AuditDAL(Model.DatabaseConnection connection, IConfiguration configuration)

    {

      _connection = connection;

      _configuration = configuration;

    }
        public List<Dictionary<string, object>> ReadAuditDropList()
        {
            try
            {
                DataTable dt = new DataTable();

                OracleParameter[] param = new OracleParameter[1];

                param[0] = new OracleParameter(
                    "p_result",
                    OracleDbType.RefCursor);

                param[0].Direction = ParameterDirection.Output;

                dt = _connection.getDataTableStoredProc(
                    "csnet_plus_master_pkg.get_audit_type_dd",
                    param);

                var result = new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();

                    foreach (DataColumn col in dt.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }

                    result.Add(dict);
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> DownloadTemplate(int auditTypeId)
        {
            using OracleConnection con = new OracleConnection(
                _configuration.GetConnectionString("DefaultConnection"));

            await con.OpenAsync();

            using OracleCommand cmd = new OracleCommand(
                "CSNET_PLUS.CSNET_PLUS_REPORT_PKG.GET_TEMPLATE_FILE_PATH",
                con);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.BindByName = true;

            cmd.Parameters.Add("p_audit_type_id", OracleDbType.Int32)
                          .Value = auditTypeId;

            cmd.Parameters.Add("p_path", OracleDbType.Varchar2, 1000)
                          .Direction = ParameterDirection.Output;

            cmd.Parameters.Add("p_msg", OracleDbType.Varchar2, 4000)
                          .Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            string message = cmd.Parameters["p_msg"].Value?.ToString();

            if (!string.IsNullOrWhiteSpace(message))
            {
                throw new Exception(message);
            }

            return cmd.Parameters["p_path"].Value?.ToString();
        }



    }
}
