using System.Data.Common;
using System.Data.SqlClient;

namespace Pomelo.AspNetCore.WebPages.Data.SqlServer
{
    public class SqlDatabase : Database
    {
        protected override void HandleLastInsertId(DbConnection conn, DbCommand cmd)
        {
            var sqlCmd = new SqlCommand("SELECT @@IDENTITY", (SqlConnection)conn);
            LastInsertId = sqlCmd.ExecuteScalar();
        }

        protected override DbCommand CreateDbCommand(string commandText, params object[] args)
        {
            var mysqlCmd = new SqlCommand(ReplaceCommandText(commandText, args), (SqlConnection)DbConnection);
            for (var i = 0; i < args.Length; i++)
                mysqlCmd.Parameters.Add(new SqlParameter { ParameterName = "@p" + i, Value = args[i] });
            return mysqlCmd;
        }
    }
}
