using System;
using System.Collections.Generic;

namespace Pomelo.AspNetCore.WebPages.Data
{
    public class DatabaseOptions
    {
        public IDictionary<string, string> ConnectionStrings { get; set; } = new Dictionary<string, string>();

        public void AddConnectionString(string connectionStr)
        {
            if (ConnectionStrings.ContainsKey("default"))
                throw new InvalidOperationException("The connection string already existed: default");
            ConnectionStrings.Add("default", connectionStr);
        }

        public void AddConnectionString(string name, string connectionStr)
        {
            if (ConnectionStrings.ContainsKey(name.ToLower()))
                throw new InvalidOperationException($"The connection string already existed: { name }");
            ConnectionStrings.Add(name.ToLower(), connectionStr);
        }
    }
}
