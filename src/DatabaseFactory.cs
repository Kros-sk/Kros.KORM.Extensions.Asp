using Kros.KORM.Extensions.Asp.Properties;
using Kros.KORM.Migrations;
using Kros.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kros.KORM.Extensions.Asp
{
    internal class DatabaseFactory : IDatabaseFactory
    {
        private static readonly ConcurrentDictionary<IServiceCollection, Dictionary<string, KormBuilder>> _builders =
            new ConcurrentDictionary<IServiceCollection, Dictionary<string, KormBuilder>>();

        private static Dictionary<string, KormBuilder> AddBuildersDictionary(IServiceCollection services)
            => _builders.GetOrAdd(services, _ => new Dictionary<string, KormBuilder>());

        /// <summary>
        /// Adds <paramref name="builder"/> with name <paramref name="name"/> into the list of builders for
        /// <paramref name="services"/>.
        /// </summary>
        /// <param name="services">Service collection for which the builder is added.</param>
        /// <param name="name">The name of the database builder.</param>
        /// <param name="builder">Database builder.</param>
        /// <returns><see langword="true"/>, if this was the first builder added, otherwise <see langword="false"/>.</returns>
        internal static void AddBuilder(IServiceCollection services, string name, KormBuilder builder)
        {
            Dictionary<string, KormBuilder> builders = AddBuildersDictionary(services);
            if (builders.ContainsKey(name))
            {
                throw new ArgumentException(string.Format(Resources.DuplicateDatabaseName, name), nameof(name));
            }
            builders.Add(name, builder);
        }

        private readonly ConcurrentDictionary<string, IDatabase> _databases = new ConcurrentDictionary<string, IDatabase>();
        private readonly Dictionary<string, KormBuilder> _localBuilders;
        private bool _disposed = false;

        internal DatabaseFactory(IServiceCollection services)
        {
            _localBuilders = AddBuildersDictionary(services);
        }

        IDatabase IDatabaseFactory.GetDatabase(string name)
        {
            KormBuilder builder = GetBuilder(name);
            return _databases.GetOrAdd(name, _ => builder.Build());
        }

        IMigrationsRunner IDatabaseFactory.GetMigrationsRunner(string name)
        {
            KormBuilder builder = GetBuilder(name);
            return builder.MigrationsRunner;
        }

        private KormBuilder GetBuilder(string name)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DatabaseFactory));
            }
            Check.NotNullOrWhiteSpace(name, nameof(name));

            if (!_localBuilders.TryGetValue(name, out KormBuilder builder))
            {
                throw new ArgumentException(
                    string.Format(Resources.InvalidDatabaseName, name, nameof(ServiceCollectionExtensions.AddKorm)),
                    nameof(name));
            }
            return builder;
        }

        public void Dispose()
        {
            _disposed = true;
            foreach (IDatabase database in _databases.Values)
            {
                database?.Dispose();
            }
            _databases.Clear();
        }
    }
}
