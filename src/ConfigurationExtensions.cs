using Kros.KORM;
using Kros.KORM.Extensions.Asp;
using Microsoft.Data.SqlClient;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Extensions for <see cref="IConfiguration"/>.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Returns connection settings with <paramref name="name"/> from configuration. Connection settings are merged from
        /// sections <c>ConnectionStrings</c> and <c>KormSettings</c>. If key <paramref name="name"/> does not exist
        /// in either section, <see langword="null"/> is returned.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="name">The connection string key.</param>
        /// <returns><see cref="KormConnectionSettings"/> or <see langword="null"/>.</returns>
        public static KormConnectionSettings GetKormConnectionString(this IConfiguration configuration, string name)
        {
            string connectionString = configuration.GetConnectionString(name);
            IConfigurationSection section = configuration.GetSection(KormBuilder.KormSettingsSectionName + ":" + name);

            if (section.Exists())
            {
                KormConnectionSettings settings = section.Get<KormConnectionSettings>();
                settings.ConnectionString = SetApplicationName(connectionString, settings.ApplicationName);
                return settings;
            }
            else if (connectionString != null)
            {
                return new KormConnectionSettings() { ConnectionString = connectionString };
            }
            return null;
        }

        static string SetApplicationName(string connectionString, string applicationName)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);

                if (string.IsNullOrEmpty(builder.ApplicationName) && !string.IsNullOrEmpty(applicationName))
                {
                    builder.ApplicationName = applicationName;
                }

                return builder.ConnectionString;
            }
            catch
            {
                return connectionString;
            }
        }
    }
}
