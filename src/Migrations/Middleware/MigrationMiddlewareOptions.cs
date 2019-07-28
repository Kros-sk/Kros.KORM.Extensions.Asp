using System;

namespace Kros.KORM.Migrations.Middleware
{
    /// <summary>
    /// Migration middleware options.
    /// </summary>
    public class MigrationMiddlewareOptions
    {
        /// <summary>
        /// Migrations endpoint URL. Default value is <c>/kormmigration</c>.
        /// </summary>
        public string EndpointUrl { get; set; } = "/kormmigration";

        /// <summary>
        /// Minimum time between two migrations.
        /// </summary>
        public TimeSpan SlidingExpirationBetweenMigrations { get; set; } = TimeSpan.FromMinutes(1);
    }
}
