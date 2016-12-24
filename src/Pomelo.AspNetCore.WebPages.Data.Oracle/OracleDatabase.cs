using System;
using System.Data.Common;
using Oracle.DataAccess.Client;

namespace Pomelo.AspNetCore.WebPages.Data.Oracle
{
    public class OracleDatabase : Database
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
            var mysqlCmd = new OracleCommand(ReplaceCommandText(commandText, args), (OracleConnection)DbConnection);
            for (var i = 0; i < args.Length; i++)
                mysqlCmd.Parameters.Add(new OracleParameter { ParameterName = "@p" + i, Value = args[i] });
            return mysqlCmd;
        }
    }
}
