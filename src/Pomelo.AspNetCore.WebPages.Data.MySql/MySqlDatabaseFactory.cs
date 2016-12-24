using MySql.Data.MySqlClient;

namespace Pomelo.AspNetCore.WebPages.Data.MySql
{
    public class MySqlDatabaseFactory : IDatabaseFactory
    {
        private readonly DatabaseOptions _databaseOptions;

        public MySqlDatabaseFactory(DatabaseOptions databaseOptions)
        {
            _databaseOptions = databaseOptions;
        }

        public Database CreateDatabase()
        {
            return new MySqlDatabase { DbConnection = new MySqlConnection(_databaseOptions.ConnectionStrings["default"]) };
        }

        public Database CreateDatabase(string name)
        {
            return new MySqlDatabase { DbConnection = new MySqlConnection(_databaseOptions.ConnectionStrings[name.ToLower()]) };
        }

        public Database CreateDatabaseByConnectionString(string connStr)
        {
            return new MySqlDatabase { DbConnection = new MySqlConnection(connStr) };
        }
    }
}
