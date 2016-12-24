using Microsoft.Data.Sqlite;

namespace Pomelo.AspNetCore.WebPages.Data.Sqlite
{
    public class SqliteDatabaseFactory : IDatabaseFactory
    {
        private readonly DatabaseOptions _databaseOptions;

        public SqliteDatabaseFactory(DatabaseOptions databaseOptions)
        {
            _databaseOptions = databaseOptions;
        }

        public Database CreateDatabase()
        {
            return new SqliteDatabase { DbConnection = new SqliteConnection(_databaseOptions.ConnectionStrings["default"]) };
        }

        public Database CreateDatabase(string name)
        {
            return new SqliteDatabase { DbConnection = new SqliteConnection(_databaseOptions.ConnectionStrings[name.ToLower()]) };
        }

        public Database CreateDatabaseByConnectionString(string connStr)
        {
            return new SqliteDatabase { DbConnection = new SqliteConnection(connStr) };
        }
    }
}
