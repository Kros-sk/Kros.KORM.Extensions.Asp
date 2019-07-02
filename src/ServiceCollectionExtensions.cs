using Kros.KORM.Extensions.Asp.Properties;
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
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Key for connection string for setting KORM database provider. If the provider is not set in connection string,
        /// Microsoft SQL Server provider is used.
        /// </summary>
        public const string KormProviderKey = "KormProvider";

        /// <summary>
        /// Key for connection string for setting if automatic migrations are enabled (<see cref="KormBuilder.Migrate()"/>).
        /// If the value is not set in connection string, automatic migrations are disabled.
        /// </summary>
        public const string KormAutoMigrateKey = "KormAutoMigrate";

        /// <summary>
        /// Register KORM into DI container. The connection string with default name
        /// (<see cref="KormBuilder.DefaultConnectionStringName"/>) from <paramref name="configuration"/> is used for database.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        public static KormBuilder AddKorm(this IServiceCollection services, IConfiguration configuration)
            => AddKorm(services, configuration, KormBuilder.DefaultConnectionStringName);

        /// <summary>
        /// Register KORM into DI container. The connection string with name <paramref name="connectionStringName"/>
        /// from <paramref name="configuration"/> is used for database.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="connectionStringName">Name of the connection string in configuration.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        public static KormBuilder AddKorm(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringName)
        {
            Check.NotNullOrWhiteSpace(connectionStringName, nameof(connectionStringName));
            string connectionString = configuration.GetConnectionString(connectionStringName);
            if (connectionString is null)
            {
                throw new ArgumentException(
                    string.Format(Resources.InvalidConnectionStringName, connectionStringName), nameof(connectionStringName));
            }
            return AddKorm(services, connectionString);
        }

        /// <summary>
        /// Register KORM for database <paramref name="connectionString"/> into DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">Connection string for database.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> or <paramref name="connectionString"/> is <see langword="null"/>;
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>
        /// The value of <paramref name="connectionString"/> is empty string, or contains only whitespace characters.
        /// </item>
        /// <item>
        /// The value of <paramref name="connectionString"/> is not empty, but contains only KORM keys. These keys are removed
        /// so resulting connection string remains empty.
        /// </item>
        /// </list>
        /// </exception>
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
                throw new ArgumentException(Resources.ConnectionStringContainsOnlyKormKeys, nameof(connectionString));
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
