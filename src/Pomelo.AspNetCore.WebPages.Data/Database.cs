using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Pomelo.AspNetCore.WebPages.Data
{
    public abstract class Database : IDatabase, IDisposable
    {
        public DbConnection DbConnection;

        protected virtual dynamic LastInsertId { get; set; }
        
        public virtual dynamic GetLastInsertId() => LastInsertId;

        public virtual dynamic QuerySingle(string commandText, params object[] args)
        {
            return Query(commandText, args).SingleOrDefault();
        }

        public async Task<dynamic> QuerySingleAsync(string commandText, params object[] args)
        {
            return (await QueryAsync(commandText, args)).SingleOrDefault();
        }

        public virtual IEnumerable<dynamic> Query(string commandText, params object[] parameters)
        {
            DbConnection.EnsureOpened();
            using (var cmd = CreateDbCommand(commandText, parameters))
            {
                cmd.Connection.EnsureOpened();
                using (var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var row = new DynamicRow();
                        for (var i = 0; i < reader.VisibleFieldCount; i++)
                            row.SetMember(reader.GetName(i), reader.GetValue(i));
                        yield return row as dynamic;
                    }
                    cmd.Connection.Close();
                }
            }
        }

        public virtual async Task<IEnumerable<dynamic>> QueryAsync(string commandText, params object[] parameters)
        {
            await DbConnection.EnsureOpenedAsync();
            using (var cmd = CreateDbCommand(commandText, parameters))
            {
                cmd.Connection.EnsureOpened();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var ret = new List<dynamic>();
                    while (reader.Read())
                    {
                        var row = new DynamicRow();
                        for (var i = 0; i < reader.VisibleFieldCount; i++)
                            row.SetMember(reader.GetName(i), reader.GetValue(i));
                        ret.Add(row as dynamic);
                    }
                    cmd.Connection.Close();
                    return ret;
                }
            }
        }
        
        public virtual dynamic QueryValue(string commandText, params object[] parameters)
        {
            DbConnection.EnsureOpened();
            using (var cmd = CreateDbCommand(commandText, parameters))
            {
                cmd.Connection.EnsureOpened();
                var ret = cmd.ExecuteScalar();
                cmd.Connection.Close();
                return ret;
            }
        }

        public virtual async Task<dynamic> QueryValueAsync(string commandText, params object[] parameters)
        {
            await DbConnection.EnsureOpenedAsync();
            using (var cmd = CreateDbCommand(commandText, parameters))
            {
                cmd.Connection.EnsureOpened();
                var ret = await cmd.ExecuteScalarAsync();
                cmd.Connection.Close();
                return ret;
            }
        }

        public virtual int Execute(string commandText, params object[] args)
        {
            DbConnection.EnsureOpened();
            using (var cmd = CreateDbCommand(commandText, args))
            {
                cmd.Connection.EnsureOpened();
                var ret = cmd.ExecuteNonQuery();
                HandleLastInsertId(DbConnection, cmd);
                cmd.Connection.Close();
                return ret;
            }
        }

        public virtual async Task<int> ExecuteAsync(string commandText, params object[] args)
        {
            await DbConnection.EnsureOpenedAsync();
            using (var cmd = CreateDbCommand(commandText, args))
            {
                cmd.Connection.EnsureOpened();
                var ret = await cmd.ExecuteNonQueryAsync();
                HandleLastInsertId(DbConnection, cmd);
                cmd.Connection.Close();
                return ret;
            }
        }

        protected abstract void HandleLastInsertId(DbConnection conn, DbCommand cmd);

        protected abstract DbCommand CreateDbCommand(string commandText, params object[] args);

        protected static string ReplaceCommandText(string commandText, params object[] args)
        {
            for (var i = 0; i < args.Length; i++)
                commandText = commandText.Replace("{" + i + "}", "@p" + i);
            return commandText;
        }

        public void Dispose()
        {
            DbConnection.Dispose();
        }

        public static IDatabaseFactory factory;

        public static Database Open(string name)
        {
            return factory.CreateDatabase(name);
        }

        public static Database Open()
        {
            return Open("default");
        }

        public static Database OpenConnectionString(string connStr)
        {
            return factory.CreateDatabaseByConnectionString(connStr);
        }
    }
}
