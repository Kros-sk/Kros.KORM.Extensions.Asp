using Kros.KORM.Migrations;
using System;

namespace Kros.KORM.Extensions.Asp
{
    /// <summary>
    /// Factory for using multiple named databases.
    /// </summary>
    public interface IDatabaseFactory : IDisposable
    {
        /// <summary>
        /// Returns database with specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the database to get.</param>
        /// <returns>The database.</returns>
        /// <exception cref="ArgumentNullException">The value of <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// The value of <paramref name="name"/> is:
        /// <list type="bullet">
        /// <item>Empty string.</item>
        /// <item>String containing whitespace characters.</item>
        /// <item>Ivalid name. The database with that name is not registered.</item>
        /// </list>
        /// </exception>
        IDatabase GetDatabase(string name);

        /// <summary>
        /// Returns the migrations runner for database with specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the database.</param>
        /// <returns>Implementation of <see cref="IMigrationsRunner"/> or <see langword="null"/>.</returns>
        /// <remarks>
        /// Migrations must be added by <see cref="KormBuilder.AddKormMigrations(Action{MigrationOptions})"/>.
        /// If migrations was not added for specified database, this method returns <see langword="null"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The value of <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// The value of <paramref name="name"/> is:
        /// <list type="bullet">
        /// <item>Empty string.</item>
        /// <item>String containing whitespace characters.</item>
        /// <item>Ivalid name. The database with that name is not registered.</item>
        /// </list>
        /// </exception>
        IMigrationsRunner GetMigrationsRunner(string name);
    }
}
