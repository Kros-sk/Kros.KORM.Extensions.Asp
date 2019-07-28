using Kros.KORM.Extensions.Asp;
using Kros.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Kros.KORM.Migrations.Middleware
{
    /// <summary>
    /// Middleware for executing database migration.
    /// </summary>
    public class MigrationsMiddleware
    {
        private const string MigrationExecutedKey = "KormMigrationsExecuted";

#pragma warning disable IDE0052 // Remove unread private members
        private readonly RequestDelegate _next;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly MigrationMiddlewareOptions _options;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationsMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next delegate.</param>
        /// <param name="cache">Memory cache.</param>
        /// <param name="options">Migration options.</param>
        public MigrationsMiddleware(RequestDelegate next, IMemoryCache cache, MigrationMiddlewareOptions options)
        {
            _next = next;
            _options = Check.NotNull(options, nameof(options));
            _cache = Check.NotNull(cache, nameof(cache));
        }

        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="databaseFactory">Database factory for getting <see cref="IMigrationsRunner"/>.</param>
        public async Task Invoke(HttpContext context, IDatabaseFactory databaseFactory)
        {
            string databaseName = null;
            if (context.Request.Path.HasValue)
            {
                databaseName = context.Request.Path.Value.Trim('/');
            }
            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = KormBuilder.DefaultConnectionStringName;
            }
            IMigrationsRunner migrationsRunner = databaseFactory.GetMigrationsRunner(databaseName);
            if ((migrationsRunner != null) && CanMigrate(databaseName))
            {
                SetupCache(databaseName);
                await migrationsRunner.MigrateAsync();
            }
        }

        private bool CanMigrate(string name) => !_cache.TryGetValue(GetCacheKey(name), out bool migrated) || !migrated;

        private void SetupCache(string name)
        {
            var options = new MemoryCacheEntryOptions();
            options.SetSlidingExpiration(_options.SlidingExpirationBetweenMigrations);
            _cache.Set(GetCacheKey(name), true, options);
        }

        private string GetCacheKey(string name) => MigrationExecutedKey + "-" + name;
    }
}
