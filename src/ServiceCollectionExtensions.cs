using Kros.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data.Common;

namespace Kros.KORM.Extensions.Asp
{
    /// <summary>
    /// Extensions for registering <see cref="IDatabase"/> into DI container.
    /// </summary>
    /// <example>
    /// "ConnectionString": {
    ///   "ProviderName": "System.Data.SqlClient",
    ///   "ConnectionString": "Server=servername\\instancename;Initial Catalog=database;Persist Security Info=False;"
    /// }
    /// </example>
    public static class ServiceCollectionExtensions
    {
        public const string KormProviderKey = "KormProvider";
        public const string KormAutoMigrateKey = "KormAutoMigrate";

        public static KormBuilder AddKorm(this IServiceCollection services, IConfiguration configuration)
            => AddKorm(services, configuration.GetConnectionString(KormBuilder.DefaultConnectionStringName));

        public static KormBuilder AddKorm(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringName)
            => AddKorm(services, configuration.GetConnectionString(connectionStringName));

        public static KormBuilder AddKorm(this IServiceCollection services, string connectionString)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));

            var cnstrBuilder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };
            string providerName = GetKormProvider(cnstrBuilder);
            bool autoMigrate = GetKormAutoMigrate(cnstrBuilder);
            connectionString = cnstrBuilder.ConnectionString; // Previous methods remove keys, so we want clean connection string.

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // RES:
                throw new ArgumentException("Connection string contained only KORM keys. These were removed and so connection is empty.", nameof(connectionString));
            }

            var builder = new KormBuilder(services, connectionString, autoMigrate, providerName);
            services.AddScoped<IDatabase>(serviceProvider => builder.Build());

            return builder;
        }

        private static string GetKormProvider(DbConnectionStringBuilder cnstrBuilder)
        {
            if (cnstrBuilder.TryGetValue(KormProviderKey, out object cnstrProviderName))
            {
                cnstrBuilder.Remove(KormProviderKey);
                string providerName = (string)cnstrProviderName;
                if (!string.IsNullOrWhiteSpace(providerName))
                {
                    return providerName;
                };
            }
            return KormBuilder.DefaultProviderName;
        }

        private static bool GetKormAutoMigrate(DbConnectionStringBuilder cnstrBuilder)
        {
            if (cnstrBuilder.TryGetValue(KormAutoMigrateKey, out object cnstrAutoMigrate))
            {
                cnstrBuilder.Remove(KormAutoMigrateKey);
                if (bool.TryParse((string)cnstrAutoMigrate, out bool autoMigrate))
                {
                    return autoMigrate;
                }
            }
            return KormBuilder.DefaultAutoMigrate;
        }
    }
}
