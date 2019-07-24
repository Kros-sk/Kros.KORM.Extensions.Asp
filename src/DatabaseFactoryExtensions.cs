namespace Kros.KORM.Extensions.Asp
{
    /// <summary>
    /// Extension methods for <see cref="IDatabaseFactory"/>.
    /// </summary>
    public static class DatabaseFactoryExtensions
    {
        /// <summary>
        /// Returns database registered with default connection string name
        /// (<see cref="KormBuilder.DefaultConnectionStringName"/>).
        /// </summary>
        /// <param name="factory">Database factory.</param>
        /// <returns>Database instance.</returns>
        public static IDatabase GetDatabase(this IDatabaseFactory factory)
            => factory.GetDatabase(KormBuilder.DefaultConnectionStringName);
    }
}
