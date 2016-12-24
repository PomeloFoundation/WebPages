using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Pomelo.AspNetCore.WebPages.Data.MySql
{
    public class MySqlDatabase : Database
    {
        protected override void HandleLastInsertId(DbConnection conn, DbCommand cmd)
        {
            LastInsertId = ((MySqlCommand)cmd).LastInsertedId;
        }

        protected override DbCommand CreateDbCommand(string commandText, params object[] args)
        {
            var mysqlCmd = new MySqlCommand(ReplaceCommandText(commandText, args), (MySqlConnection)DbConnection);
            for (var i = 0; i < args.Length; i++)
                mysqlCmd.Parameters.Add(new MySqlParameter { ParameterName = "@p" + i, Value = args[i] });
            return mysqlCmd;
        }
    }
}
