using Npgsql;

namespace Pomelo.AspNetCore.WebPages.Data.Npgsql
{
    public class NpgsqlDatabaseFactory : IDatabaseFactory
    {
        private readonly DatabaseOptions _databaseOptions;

        public NpgsqlDatabaseFactory(DatabaseOptions databaseOptions)
        {
            _databaseOptions = databaseOptions;
        }

        public Database CreateDatabase()
        {
            return new NpgsqlDatabase { DbConnection = new NpgsqlConnection(_databaseOptions.ConnectionStrings["default"]) };
        }

        public Database CreateDatabase(string name)
        {
            return new NpgsqlDatabase { DbConnection = new NpgsqlConnection(_databaseOptions.ConnectionStrings[name.ToLower()]) };
        }

        public Database CreateDatabaseByConnectionString(string connStr)
        {
            return new NpgsqlDatabase { DbConnection = new NpgsqlConnection(connStr) };
        }
    }
}
