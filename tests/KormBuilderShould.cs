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

            var exception = Assert.Throws<ArgumentNullException>(() => new KormBuilder(null, connectionString));
            Assert.Equal("services", exception.ParamName);

            exception = Assert.Throws<ArgumentNullException>(() => new KormBuilder(services, (KormConnectionSettings)null));
            Assert.Equal("connectionSettings", exception.ParamName);

            exception = Assert.Throws<ArgumentNullException>(() => new KormBuilder(services, (string)null));
            Assert.Equal("connectionSettings", exception.ParamName);

            var argumentException = Assert.Throws<ArgumentException>(() => new KormBuilder(services, string.Empty));
            Assert.Equal("connectionSettings", argumentException.ParamName);

            argumentException = Assert.Throws<ArgumentException>(() => new KormBuilder(services, " \t "));
            Assert.Equal("connectionSettings", argumentException.ParamName);
        }

        [Fact]
        public void UseDatabaseConfiguration()
        {
            KormBuilder kormBuilder = CreateKormBuilder(false);
            DatabaseConfigurationBase configuration = Substitute.For<DatabaseConfigurationBase>();

            kormBuilder.UseDatabaseConfiguration(configuration);
            IDatabase database = kormBuilder.Build();

            Assert.NotNull(database);
            configuration.Received().OnModelCreating(Arg.Any<ModelConfigurationBuilder>());
        }

        private static KormBuilder CreateKormBuilder(bool autoMigrate)
            => new KormBuilder(new ServiceCollection(), $"server=localhost;KormAutoMigrate={autoMigrate}");
    }
}
