using FluentAssertions;
using Kros.KORM.Extensions.Asp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class ServiceCollectionExtensionsShould
    {
        [Fact]
        public void AddKormToContainer()
        {
            var services = new ServiceCollection();

            services.AddKorm("server=localhost");

            services.BuildServiceProvider()
                .GetService<IDatabase>()
                .Should().NotBeNull();
        }

        [Fact]
        public void UseDefaultConnectionStringIfConfigurationIsProvided()
        {
            IConfigurationRoot configuration = ConfigurationHelper.GetConfiguration();
            var services = new ServiceCollection();

            KormBuilder builder = services.AddKorm(configuration);

            builder.ConnectionString.Should().Be(configuration.GetConnectionString("DefaultConnection"));
        }

        [Fact]
        public void ThrowArgumentExceptionIfInvalidConnectionStringName()
        {
            const string cnstrName = "NonExistingName";

            Action action = () =>
            {
                IConfigurationRoot configuration = ConfigurationHelper.GetConfiguration();
                var services = new ServiceCollection();

                services.AddKorm(configuration, cnstrName);
            };
            action.Should().Throw<ArgumentException>().WithMessage($"*{cnstrName}*");
        }

        [Fact]
        public void ThrowArgumentExceptionIfConnectionStringContainsOnlyKormValues()
        {
            Action action = () =>
            {
                var services = new ServiceCollection();
                services.AddKorm("KormProvider=LoremIpsum;KormAutoMigrate=false");
            };

            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("connectionString");
        }

        [Theory]
        [InlineData("server=localhost;", KormBuilder.DefaultProviderName, false)]
        [InlineData("server=localhost;KormProvider=", KormBuilder.DefaultProviderName, false)]
        [InlineData("server=localhost;KormProvider=' \t '", KormBuilder.DefaultProviderName, false)]
        [InlineData("server=localhost;KormProvider=LoremIpsum", "LoremIpsum", false)]
        [InlineData("server=localhost;KormAutoMigrate=true", KormBuilder.DefaultProviderName, true)]
        [InlineData("server=localhost;KormAutoMigrate=false", KormBuilder.DefaultProviderName, false)]
        [InlineData("server=localhost;KormAutoMigrate=InvalidValue", KormBuilder.DefaultProviderName, false)]
        [InlineData("server=localhost;KormAutoMigrate=", KormBuilder.DefaultProviderName, false)]
        [InlineData("server=localhost;KormAutoMigrate=' \t '", KormBuilder.DefaultProviderName, false)]
        public void ParseKormConnectionStringKeys(string connectionString, string provider, bool autoMigrate)
        {
            var services = new ServiceCollection();

            KormBuilder builder = services.AddKorm(connectionString);

            builder.KormProvider.Should().Be(provider);
            builder.AutoMigrate.Should().Be(autoMigrate);
            builder.ConnectionString.Should().Be("server=localhost");
        }
    }
}
