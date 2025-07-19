using System;
using Microsoft.Extensions.Configuration;

namespace HodHod.Configuration
{
    public static class ConnectionStringProvider
    {
        public static string Get(IConfiguration configuration)
        {
            // Check well known environment variables first
            string[] names = new[]
            {
                "DB_CONNECTION_STRING",
                $"ConnectionStrings__{HodHodConsts.ConnectionStringName}",
                $"ConnectionStrings:{HodHodConsts.ConnectionStringName}",
                HodHodConsts.ConnectionStringName,
                $"{HodHodConsts.ConnectionStringName}_CONNECTION_STRING",
                "DefaultConnection",
                "Default"
            };

            foreach (var name in names)
            {
                var value = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            // Fallback to configuration files
            var fromConfig = configuration.GetConnectionString(HodHodConsts.ConnectionStringName);
            if (!string.IsNullOrWhiteSpace(fromConfig))
            {
                return fromConfig;
            }

            return configuration[$"ConnectionStrings:{HodHodConsts.ConnectionStringName}"];
        }
    }
}