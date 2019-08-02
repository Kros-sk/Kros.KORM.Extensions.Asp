using FluentAssertions;
using Kros.KORM.Extensions.Asp;
using Kros.KORM.Metadata;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class KormBuilderShould
    {
        [Fact]
        public void ThrowArgumentExceptionWhenArgumentsAreInvalid()
        {
            const string connectionString = "server=localhost";
            var services = new ServiceCollection();

            Action action = () => new KormBuilder(null, connectionString);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("services");

            action = () => new KormBuilder(services, (KormConnectionSettings)null);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("connectionSettings");

            action = () => new KormBuilder(services, (string)null);
            action.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("connectionSettings");

            action = () => new KormBuilder(services, string.Empty);
            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("connectionSettings");

            action = () => new KormBuilder(services, " \t ");
            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("connectionSettings");
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
            => new KormBuilder(new ServiceCollection(), $"server=localhost;KormAutoMigrate={autoMigrate}");
    }
}
