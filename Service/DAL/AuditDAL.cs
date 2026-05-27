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
    private OracleConnection objConn;

    private OracleCommand objCmd;

    private OracleDataAdapter objDA;

        

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

        public List<Dictionary<string, object>> DownloadTemplate()
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
                    "csnet_plus_master_pkg.get_claim_upload_template",
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



    }
}
