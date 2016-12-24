using Oracle.DataAccess.Client;

namespace Pomelo.AspNetCore.WebPages.Data.Oracle
{
    public class OracleDatabaseFactory : IDatabaseFactory
    {
        private readonly DatabaseOptions _databaseOptions;

        public OracleDatabaseFactory(DatabaseOptions databaseOptions)
        {
            _databaseOptions = databaseOptions;
        }

        public Database CreateDatabase()
        {
            return new OracleDatabase { DbConnection = new OracleConnection(_databaseOptions.ConnectionStrings["default"]) };
        }

        public Database CreateDatabase(string name)
        {
            return new OracleDatabase { DbConnection = new OracleConnection(_databaseOptions.ConnectionStrings[name.ToLower()]) };
        }

        public Database CreateDatabaseByConnectionString(string connStr)
        {
            return new OracleDatabase { DbConnection = new OracleConnection(connStr) };
        }
    }
}
