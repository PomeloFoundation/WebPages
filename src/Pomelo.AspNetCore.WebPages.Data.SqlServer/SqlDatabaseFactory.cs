using System.Data.SqlClient;

namespace Pomelo.AspNetCore.WebPages.Data.SqlServer
{
    public class SqlDatabaseFactory : IDatabaseFactory
    {
        private readonly DatabaseOptions _databaseOptions;

        public SqlDatabaseFactory(DatabaseOptions databaseOptions)
        {
            _databaseOptions = databaseOptions;
        }

        public Database CreateDatabase()
        {
            return new SqlDatabase { DbConnection = new SqlConnection(_databaseOptions.ConnectionStrings["default"]) };
        }

        public Database CreateDatabase(string name)
        {
            return new SqlDatabase { DbConnection = new SqlConnection(_databaseOptions.ConnectionStrings[name.ToLower()]) };
        }

        public Database CreateDatabaseByConnectionString(string connStr)
        {
            return new SqlDatabase { DbConnection = new SqlConnection(connStr) };
        }
    }
}
