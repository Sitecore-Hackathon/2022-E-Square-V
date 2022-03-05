using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mvp.Project.MvpSite.Configuration
{
    public class RedisConfiguration
    {
        public static string Key = "Redis";

        public string ConnectionString { get; set; }
    }
}
