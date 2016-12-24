using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Pomelo.AspNetCore.WebPages.Data.Sqlite
{
    public class SqliteDatabase : Database
    {
        protected override void HandleLastInsertId(DbConnection conn, DbCommand cmd)
        {
            var sqliteCmd = new SqliteCommand("SELECT LAST_INSERT_ROWID()", (SqliteConnection)conn);
            LastInsertId = sqliteCmd.ExecuteScalar();
            return;
        }

        protected override DbCommand CreateDbCommand(string commandText, params object[] args)
        {
            var mysqlCmd = new SqliteCommand(ReplaceCommandText(commandText, args), (SqliteConnection)DbConnection);
            for (var i = 0; i < args.Length; i++)
                mysqlCmd.Parameters.Add(new SqliteParameter { ParameterName = "@p" + i, Value = args[i] });
            return mysqlCmd;
        }
    }
}
