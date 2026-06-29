using System.Data.Common;
using Oracle.ManagedDataAccess.Types;

namespace CallAuditPortal1.Model
{
    public static class DBHelper
    {
        public static string? GetString(DbParameterCollection parameters, string parameterName)
        {
            var value = parameters[parameterName].Value;

            Console.WriteLine(value == null);          // true or false
            Console.WriteLine(value == DBNull.Value);  // true or false
            Console.WriteLine(value?.GetType().FullName);

            if (value == null || value == DBNull.Value)
                return null;
            if (value is OracleString oracleString)
                return oracleString.IsNull ? null : oracleString.Value;
            return value.ToString();
        }

        public static int? GetInt(DbParameterCollection parameters, string parameterName)
        {
            var value = parameters[parameterName].Value;
            return value == DBNull.Value ? null : Convert.ToInt32(value);
        }

        public static decimal? GetDecimal(DbParameterCollection parameters, string parameterName)
        {
            var value = parameters[parameterName].Value;
            return value == DBNull.Value ? null : Convert.ToDecimal(value);
        }

        public static DateTime? GetDateTime(DbParameterCollection parameters, string parameterName)
        {
            var value = parameters[parameterName].Value;
            return value == DBNull.Value ? null : Convert.ToDateTime(value);
        }
    }
}
