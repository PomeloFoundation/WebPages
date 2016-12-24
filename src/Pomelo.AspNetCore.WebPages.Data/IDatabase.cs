using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pomelo.AspNetCore.WebPages.Data
{
    public interface IDatabase : IDisposable
    {
        dynamic QuerySingle(string commandText, params object[] args);

        Task<dynamic> QuerySingleAsync(string commandText, params object[] args);

        IEnumerable<dynamic> Query(string commandText, params object[] parameters);

        Task<IEnumerable<dynamic>> QueryAsync(string commandText, params object[] parameters);

        dynamic QueryValue(string commandText, params object[] parameters);

        Task<dynamic> QueryValueAsync(string commandText, params object[] parameters);

        int Execute(string commandText, params object[] args);

        Task<int> ExecuteAsync(string commandText, params object[] args);
    }
}
