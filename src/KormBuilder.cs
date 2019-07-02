using Kros.Data;
using Kros.KORM.Migrations;
using Kros.KORM.Migrations.Middleware;
using Kros.KORM.Migrations.Providers;
using Kros.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Kros.KORM.Extensions.Asp
{
    /// <summary>
    /// Builder for initialization of <see cref="IDatabase"/>.
    /// </summary>
    public class KormBuilder
    {
        public const string DefaultConnectionStringName = "DefaultConnection";
        internal const string DefaultProviderName = Kros.Data.SqlServer.SqlServerDataHelper.ClientId;
        internal const bool DefaultAutoMigrate = false;

        private readonly IDatabaseBuilder _builder;

        public KormBuilder(IServiceCollection services, string connectionString)
            : this(services, connectionString, DefaultAutoMigrate, DefaultProviderName)
        {
        }

        public KormBuilder(IServiceCollection services, string connectionString, bool autoMigrate)
            : this(services, connectionString, autoMigrate, DefaultProviderName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KormBuilder"/> class.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="connectionString">The connection string settings.</param>
        public KormBuilder(IServiceCollection services, string connectionString, bool autoMigrate, string kormProvider)
        {
            Services = Check.NotNull(services, nameof(services));
            ConnectionString = Check.NotNullOrWhiteSpace(connectionString, nameof(connectionString));
            KormProvider = Check.NotNullOrWhiteSpace(kormProvider, nameof(kormProvider));
            AutoMigrate = autoMigrate;

            _builder = Database.Builder;
            _builder.UseConnection(connectionString, kormProvider);
        }

        /// <summary>
        /// Gets the service collection.
        /// </summary>
        public IServiceCollection Services { get; }

        internal string ConnectionString { get; }
        internal string KormProvider { get; }
        internal bool AutoMigrate { get; }

        /// <summary>
        /// Use database configuration.
        /// </summary>
        /// <typeparam name="TConfiguration">Configuration type.</typeparam>
        /// <returns>KORM builder.</returns>
        public KormBuilder UseDatabaseConfiguration<TConfiguration>() where TConfiguration : DatabaseConfigurationBase, new()
        {
            _builder.UseDatabaseConfiguration<TConfiguration>();
            return this;
        }

        /// <summary>
        /// Use database configuration.
        /// </summary>
        /// <param name="databaseConfiguration">Instance of database configuration.</param>
        /// <returns>KORM builder.</returns>
        public KormBuilder UseDatabaseConfiguration(DatabaseConfigurationBase databaseConfiguration)
        {
            _builder.UseDatabaseConfiguration(databaseConfiguration);
            return this;
        }

        /// <summary>
        /// Initializes database for using Id generator.
        /// </summary>
        /// <returns>This instance.</returns>
        public KormBuilder InitDatabaseForIdGenerator()
        {
            IIdGeneratorFactory factory = IdGeneratorFactories.GetFactory(ConnectionString, KormProvider);
            using (IIdGenerator idGenerator = factory.GetGenerator(string.Empty))
            {
                idGenerator.InitDatabaseForIdGenerator();
            }
            return this;
        }

        /// <summary>
        /// Adds configuration for <see cref="MigrationsMiddleware"/> into <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="setupAction">Setup migration options.</param>
        /// <returns>This instance of <see cref="KormBuilder"/>.</returns>
        public KormBuilder AddKormMigrations(Action<MigrationOptions> setupAction = null)
        {
            Services
                .AddMemoryCache()
                .AddTransient((Func<IServiceProvider, IMigrationsRunner>)(s =>
                {
                    var database = new Database(ConnectionString, KormProvider);
                    MigrationOptions options = SetupMigrationOptions(setupAction);
                    return new MigrationsRunner(database, options);
                }));

            return this;
        }

        private static MigrationOptions SetupMigrationOptions(Action<MigrationOptions> setupAction)
        {
            var options = new MigrationOptions();

            if (setupAction != null)
            {
                setupAction.Invoke(options);
            }
            else
            {
                options.AddScriptsProvider(AssemblyMigrationScriptsProvider.GetEntryAssemblyProvider());
            }

            return options;
        }

        /// <summary>
        /// Execute database migration.
        /// </summary>
        public void Migrate()
        {
            if (AutoMigrate)
            {
                Services.BuildServiceProvider()
                    .GetService<IMigrationsRunner>()
                    .MigrateAsync()
                    .Wait();
            }
        }

        internal IDatabase Build() => _builder.Build();
    }
}
