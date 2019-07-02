using FluentAssertions;
using Kros.KORM.Extensions.Asp;
using Kros.KORM.Metadata;
using Kros.KORM.Migrations;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class KormBuilderShould
    {
        [Theory]
        [InlineData(typeof(string))]
        public void asdf(Type t)
        {

        }

        [Fact]
        public void ThrowArgumentExceptionWhenArgumentsAreInvalid()
        {
            const string connectionString = "server=localhost";
            var services = new ServiceCollection();

            Action action = () => new KormBuilder(null, connectionString);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");

            action = () => new KormBuilder(services, null);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("connectionString");

            action = () => new KormBuilder(services, string.Empty);
            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("connectionString");

            action = () => new KormBuilder(services, " \t ");
            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("connectionString");

            action = () => new KormBuilder(services, connectionString, false, null);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("kormProvider");

            action = () => new KormBuilder(services, connectionString, false, string.Empty);
            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("kormProvider");

            action = () => new KormBuilder(services, connectionString, false, " \t ");
            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("kormProvider");
        }

        [Fact]
        public void AddMigrationsToContainer()
        {
            KormBuilder kormBuilder = CreateKormBuilder(false);
            kormBuilder.AddKormMigrations();

            kormBuilder.Services.BuildServiceProvider()
                .GetService<IMigrationsRunner>()
                .Should().NotBeNull();
        }

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void ExecuteMigrationsBasedOnAutoMigrateValue(bool autoMigrate, int migrateCallCount)
        {
            KormBuilder kormBuilder = CreateKormBuilder(autoMigrate);
            kormBuilder.AddKormMigrations();

            IMigrationsRunner migrationRunner = Substitute.For<IMigrationsRunner>();
            kormBuilder.Services.AddSingleton(migrationRunner);

            kormBuilder.Migrate();

            migrationRunner.Received(migrateCallCount).MigrateAsync();
        }

        [Fact]
        public void UseDatabaseConfiguration()
        {
            KormBuilder kormBuilder = CreateKormBuilder(false);
            DatabaseConfigurationBase configuration = Substitute.For<DatabaseConfigurationBase>();

            kormBuilder.UseDatabaseConfiguration(configuration);
            IDatabase database = kormBuilder.Build();

            database.Should().NotBeNull();
            configuration.Received().OnModelCreating(Arg.Any<ModelConfigurationBuilder>());
        }

        private KormBuilder CreateKormBuilder(bool autoMigrate)
            => new KormBuilder(new ServiceCollection(), "server=localhost", autoMigrate);
    }
}
