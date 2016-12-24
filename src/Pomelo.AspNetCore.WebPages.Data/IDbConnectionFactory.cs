using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;

namespace Pomelo.AspNetCore.WebPages.Data
{
    public interface IDatabaseFactory
    {
        Database CreateDatabase();
        Database CreateDatabase(string name);
        Database CreateDatabaseByConnectionString(string connStr);
    }
}
