using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace CallAuditPortal1.Model
{
  public class DatabaseConnection
  {
    private readonly IConfiguration _configuration;
    public DatabaseConnection(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public DatabaseConnection(string connectionString)
    {
      this.connectionString = connectionString;
    }

    private OracleConnection objConn;
    private OracleCommand objCmd;
    private OracleDataAdapter objDA;
    private string connectionString;

    public DataSet GetDataSetProc(string strSPName, OracleParameter[] arProcParams)
    {
      using (OracleConnection conn = new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
      using (OracleCommand cmd = new OracleCommand(strSPName, conn))
      using (OracleDataAdapter da = new OracleDataAdapter(cmd))
      {
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddRange(arProcParams);
        DataSet ds = new DataSet();
        conn.Open();
        da.Fill(ds);
        return ds;
      }
    }
    public int ExecuteStoredProc(string strProcName, OracleParameter[] arProcParams)
    {
      int returnValue;

      using (OracleConnection conn = new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
      using (OracleCommand cmd = new OracleCommand(strProcName, conn))
      {
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddRange(arProcParams);

        conn.Open();
        returnValue = cmd.ExecuteNonQuery();
      }
      return returnValue;
    }

    public DataTable getDataTableStoredProc(string procName, OracleParameter[] parameters)
    {
      using (OracleConnection conn = new OracleConnection(_configuration.GetConnectionString("DefaultConnection")))
      //using (OracleConnection conn = new OracleConnection(_configuration.GetConnectionString("OracleConnection")))
      using (OracleCommand cmd = new OracleCommand(procName, conn))
      using (OracleDataAdapter da = new OracleDataAdapter(cmd))
      {
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddRange(parameters);
        DataTable dt = new DataTable();
        conn.Open();
        da.Fill(dt);
        return dt;
      }
    }

  }
}
