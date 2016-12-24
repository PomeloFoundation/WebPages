using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;

namespace Pomelo.AspNetCore.WebPages.Data
{
    public static class DbConnectionExtensions
    {
        public static void EnsureOpened(this DbConnection self)
        {
            self.Open();
        }

        public static Task EnsureOpenedAsync(this DbConnection self)
        {
            return self.OpenAsync();
        }
    }
}
