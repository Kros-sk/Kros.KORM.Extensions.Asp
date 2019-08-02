using Kros.KORM.Extensions.Asp.Properties;
using Kros.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Kros.KORM.Extensions.Asp
{
    /// <summary>
    /// Extensions for registering <see cref="IDatabase"/> into DI container.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register KORM into DI container. The connection string with default name
        /// (<see cref="KormBuilder.DefaultConnectionStringName"/>) from <paramref name="configuration"/> is used for database.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        /// <remarks>
        /// <para>
        /// Method adds an <see cref="IDatabaseFactory"/> as scoped dependency into DI container.
        /// Database (<see cref="IDatabase"/>) can be retrieved using that factory. Databases are identified by names
        /// (connection string names in <c>appsettings</c>).
        /// </para>
        /// <para>
        /// <b>The first</b> database added by any <see cref="AddKorm(IServiceCollection, IConfiguration)" autoupgrade="true"/>
        /// method is registered in the DI container also as <see cref="IDatabase"/> scoped dependency. So this database can be
        /// used directly as <see cref="IDatabase"/>, without using <see cref="IDatabaseFactory"/>.
        /// </para>
        /// </remarks>
        public static KormBuilder AddKorm(this IServiceCollection services, IConfiguration configuration)
            => AddKorm(services, configuration, KormBuilder.DefaultConnectionStringName);

        /// <summary>
        /// Register KORM into DI container. The connection string with name <paramref name="name"/>
        /// from <paramref name="configuration"/> is used for database.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="name">Name of the connection string in configuration.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        /// <remarks>
        /// <para>
        /// Method adds an <see cref="IDatabaseFactory"/> as scoped dependency into DI container.
        /// Database (<see cref="IDatabase"/>) can be retrieved using that factory. Databases are identified by names
        /// (connection string names in <c>appsettings</c>).
        /// </para>
        /// <para>
        /// <b>The first</b> database added by any <see cref="AddKorm(IServiceCollection, IConfiguration)" autoupgrade="true"/>
        /// method is registered in the DI container also as <see cref="IDatabase"/> scoped dependency. So this database can be
        /// used directly as <see cref="IDatabase"/>, without using <see cref="IDatabaseFactory"/>.
        /// </para>
        /// </remarks>
        public static KormBuilder AddKorm(this IServiceCollection services, IConfiguration configuration, string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            KormConnectionSettings connectionSettings = configuration.GetKormConnectionString(name);
            if (connectionSettings is null)
            {
                throw new ArgumentException(string.Format(Resources.InvalidConnectionStringName, name), nameof(name));
            }
            return AddKormBuilder(services, name, connectionSettings);
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
        /// <remarks>
        /// <para>
        /// Method adds an <see cref="IDatabaseFactory"/> as scoped dependency into DI container.
        /// Database (<see cref="IDatabase"/>) can be retrieved using that factory. Databases are identified by names
        /// (connection string names in <c>appsettings</c>).
        /// </para>
        /// <para>
        /// <b>The first</b> database added by any <see cref="AddKorm(IServiceCollection, IConfiguration)" autoupgrade="true"/>
        /// method is registered in the DI container also as <see cref="IDatabase"/> scoped dependency. So this database can be
        /// used directly as <see cref="IDatabase"/>, without using <see cref="IDatabaseFactory"/>.
        /// </para>
        /// </remarks>
        public static KormBuilder AddKorm(this IServiceCollection services, string connectionString)
            => AddKorm(services, connectionString, KormBuilder.DefaultConnectionStringName);

        /// <summary>
        /// Register KORM for database <paramref name="connectionString"/> into DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">Connection string for database.</param>
        /// <param name="name">Name of the database. When the database is added from <c>appsettings</c>,
        /// this is the connection string name in the settings.</param>
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
        /// <remarks>
        /// <para>
        /// Method adds an <see cref="IDatabaseFactory"/> as scoped dependency into DI container.
        /// Database (<see cref="IDatabase"/>) can be retrieved using that factory. Databases are identified by names
        /// (connection string names in <c>appsettings</c>).
        /// </para>
        /// <para>
        /// <b>The first</b> database added by any <see cref="AddKorm(IServiceCollection, IConfiguration)" autoupgrade="true"/>
        /// method is registered in the DI container also as <see cref="IDatabase"/> scoped dependency. So this database can be
        /// used directly as <see cref="IDatabase"/>, without using <see cref="IDatabaseFactory"/>.
        /// </para>
        /// </remarks>
        public static KormBuilder AddKorm(this IServiceCollection services, string connectionString, string name)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            Check.NotNullOrWhiteSpace(name, nameof(name));
            return AddKormBuilder(services, name, new KormConnectionSettings() { ConnectionString = connectionString });
        }

        /// <summary>
        /// Register KORM for database <paramref name="connectionSettings"/> into DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionSettings">Connection settings for database.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of any argument is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <remarks>
        /// <para>
        /// Method adds an <see cref="IDatabaseFactory"/> as scoped dependency into DI container.
        /// Database (<see cref="IDatabase"/>) can be retrieved using that factory. Databases are identified by names
        /// (connection string names in <c>appsettings</c>).
        /// </para>
        /// <para>
        /// <b>The first</b> database added by any <see cref="AddKorm(IServiceCollection, IConfiguration)" autoupgrade="true"/>
        /// method is registered in the DI container also as <see cref="IDatabase"/> scoped dependency. So this database can be
        /// used directly as <see cref="IDatabase"/>, without using <see cref="IDatabaseFactory"/>.
        /// </para>
        /// </remarks>
        public static KormBuilder AddKorm(this IServiceCollection services, KormConnectionSettings connectionSettings)
            => AddKorm(services, connectionSettings, KormBuilder.DefaultConnectionStringName);

        /// <summary>
        /// Register KORM for database <paramref name="connectionSettings"/> into DI container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionSettings">Connection settings for database.</param>
        /// <param name="name">Name of the database. When the database is added from <c>appsettings</c>,
        /// this is the connection string name in the settings.</param>
        /// <returns><see cref="KormBuilder"/> for <see cref="IDatabase"/> initialization.</returns>
        /// <exception cref="ArgumentNullException">
        /// The value of any argument is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>
        /// The value of <paramref name="name"/> is empty string, or contains only whitespace characters.
        /// </item>
        /// </list>
        /// </exception>
        /// <remarks>
        /// <para>
        /// Method adds an <see cref="IDatabaseFactory"/> as scoped dependency into DI container.
        /// Database (<see cref="IDatabase"/>) can be retrieved using that factory. Databases are identified by names
        /// (connection string names in <c>appsettings</c>).
        /// </para>
        /// <para>
        /// <b>The first</b> database added by any <see cref="AddKorm(IServiceCollection, IConfiguration)" autoupgrade="true"/>
        /// method is registered in the DI container also as <see cref="IDatabase"/> scoped dependency. So this database can be
        /// used directly as <see cref="IDatabase"/>, without using <see cref="IDatabaseFactory"/>.
        /// </para>
        /// </remarks>
        public static KormBuilder AddKorm(
            this IServiceCollection services,
            KormConnectionSettings connectionSettings,
            string name)
        {
            Check.NotNull(services, nameof(services));
            Check.NotNull(connectionSettings, nameof(connectionSettings));
            Check.NotNullOrWhiteSpace(name, nameof(name));
            return AddKormBuilder(services, name, connectionSettings);
        }

        private static KormBuilder AddKormBuilder(
            IServiceCollection services,
            string name,
            KormConnectionSettings connectionSettings)
        {
            AddDatabaseFactory(services);
            var builder = new KormBuilder(services, connectionSettings);
            DatabaseFactory.AddBuilder(services, name, builder);
            services.TryAdd(ServiceDescriptor.Scoped<IDatabase>(
                serviceProvider => serviceProvider.GetRequiredService<IDatabaseFactory>().GetDatabase(name)));
            return builder;
        }

        private static void AddDatabaseFactory(IServiceCollection services)
            => services.TryAdd(ServiceDescriptor.Scoped<IDatabaseFactory>(provider => new DatabaseFactory(services)));
    }
}
