using System;
using System.Collections.Generic;
using System.Data.Common;
using Npgsql;

namespace Pomelo.AspNetCore.WebPages.Data.Npgsql
{
    public class NpgsqlDatabase : Database
    {
        public override dynamic GetLastInsertId()
        {
            throw new NotSupportedException();
        }

        protected override void HandleLastInsertId(DbConnection conn, DbCommand cmd)
        {
            return;
        }

        protected override DbCommand CreateDbCommand(string commandText, params object[] args)
        {
            var mysqlCmd = new NpgsqlCommand(ReplaceCommandText(commandText, args), (NpgsqlConnection)DbConnection);
            for (var i = 0; i < args.Length; i++)
                mysqlCmd.Parameters.Add(new NpgsqlParameter("@p" + i, args[i]));
            return mysqlCmd;
        }
    }
}
