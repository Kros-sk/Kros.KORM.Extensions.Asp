using FluentAssertions;
using Kros.KORM.Extensions.Asp;
using Kros.KORM.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class ServiceCollectionExtensionsShould
    {
        private const string DefaultProviderName = Kros.Data.SqlServer.SqlServerDataHelper.ClientId;

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

            builder.ConnectionSettings.ConnectionString.Should().Be(configuration.GetConnectionString("DefaultConnection"));
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
        public void ThrowArgumentExceptionIfDefaultConnectionStringNameDoesNotExist()
        {
            Action action = () =>
            {
                IConfigurationRoot configuration = new ConfigurationBuilder().Build();
                var services = new ServiceCollection();

                services.AddKorm(configuration);
            };
            action.Should().Throw<ArgumentException>().WithMessage($"*{KormBuilder.DefaultConnectionStringName}*");
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
        [InlineData("server=localhost;", DefaultProviderName, false)]
        [InlineData("server=localhost;KormProvider=", DefaultProviderName, false)]
        [InlineData("server=localhost;KormProvider=' \t '", DefaultProviderName, false)]
        [InlineData("server=localhost;KormProvider=LoremIpsum", "LoremIpsum", false)]
        [InlineData("server=localhost;KormAutoMigrate=true", DefaultProviderName, true)]
        [InlineData("server=localhost;KormAutoMigrate=false", DefaultProviderName, false)]
        [InlineData("server=localhost;KormAutoMigrate=InvalidValue", DefaultProviderName, false)]
        [InlineData("server=localhost;KormAutoMigrate=", DefaultProviderName, false)]
        [InlineData("server=localhost;KormAutoMigrate=' \t '", DefaultProviderName, false)]
        public void ParseKormConnectionStringKeys(string connectionString, string provider, bool autoMigrate)
        {
            var services = new ServiceCollection();

            KormBuilder builder = services.AddKorm(connectionString);

            builder.ConnectionSettings.KormProvider.Should().Be(provider);
            builder.ConnectionSettings.AutoMigrate.Should().Be(autoMigrate);
            builder.ConnectionSettings.ConnectionString.Should().Be("server=localhost");
        }

        [Fact]
        public void AddFirstDatabaseAsIDatabaseToServiceCollection()
        {
            var services = new ServiceCollection();
            DatabaseConfigurationBase config1 = Substitute.For<DatabaseConfigurationBase>();
            DatabaseConfigurationBase config2 = Substitute.For<DatabaseConfigurationBase>();
            DatabaseConfigurationBase config3 = Substitute.For<DatabaseConfigurationBase>();
            services.AddKorm("server=localhost-1", "db1").UseDatabaseConfiguration(config1);
            services.AddKorm("server=localhost-2").UseDatabaseConfiguration(config2);
            services.AddKorm("server=localhost-3", "db3").UseDatabaseConfiguration(config3);

            ServiceProvider provider = services.BuildServiceProvider();
            _ = provider.GetService<IDatabase>();

            config1.Received().OnModelCreating(Arg.Any<ModelConfigurationBuilder>());
            config2.DidNotReceive().OnModelCreating(Arg.Any<ModelConfigurationBuilder>());
            config3.DidNotReceive().OnModelCreating(Arg.Any<ModelConfigurationBuilder>());
        }

        [Fact]
        public void AddDatabaseFactoryAsScopedDependency()
        {
            var services = new ServiceCollection();
            services.AddKorm("server=localhost-1", "db1");
            services.AddKorm("server=localhost-2");
            services.AddKorm("server=localhost-3", "db3");
            ServiceProvider provider = services.BuildServiceProvider();

            IDatabaseFactory factory1 = provider.GetService<IDatabaseFactory>();
            IDatabaseFactory factory2 = provider.GetService<IDatabaseFactory>();
            IDatabaseFactory factory3 = null;
            using (IServiceScope scope = provider.CreateScope())
            {
                factory3 = scope.ServiceProvider.GetService<IDatabaseFactory>();
            }

            factory2.Should().Be(factory1, "\"factory2\" was created in the same scope as \"factory1\".");
            factory3.Should().NotBeNull().And.NotBe(factory1, "\"factory3\" was created in different scope as \"factory1\".");
        }

        [Fact]
        public void ReturnTheSameDatabaseFromFactoryInTheSameScope()
        {
            var services = new ServiceCollection();

            services.AddKorm("server=localhost-1", "db1");
            services.AddKorm("server=localhost-2");
            services.AddKorm("server=localhost-3", "db3");

            ServiceProvider provider = services.BuildServiceProvider();
            IDatabaseFactory factory1 = provider.GetService<IDatabaseFactory>();
            IDatabase db1 = factory1.GetDatabase("db1");
            IDatabase db2 = factory1.GetDatabase("db1");
            IDatabase db3 = null;
            using (IServiceScope scope = provider.CreateScope())
            {
                IDatabaseFactory factory2 = scope.ServiceProvider.GetService<IDatabaseFactory>();
                db3 = factory2.GetDatabase("db1");
            }

            db2.Should().Be(db1, "\"db2\" was created using the same factory as \"db1\".");
            db3.Should().NotBeNull().And.NotBe(db1, "\"db3\" was created using factory in different scope as \"db1\".");
        }

        [Fact]
        public void ReturnDatabaseWithDefaultName()
        {
            var services = new ServiceCollection();
            DatabaseConfigurationBase config1 = Substitute.For<DatabaseConfigurationBase>();
            DatabaseConfigurationBase config2 = Substitute.For<DatabaseConfigurationBase>();
            services.AddKorm("server=localhost-1", "db1").UseDatabaseConfiguration(config1);
            services.AddKorm("server=localhost-2").UseDatabaseConfiguration(config2);

            ServiceProvider provider = services.BuildServiceProvider();
            IDatabaseFactory factory = provider.GetService<IDatabaseFactory>();

            _ = factory.GetDatabase();
            config2.Received().OnModelCreating(Arg.Any<ModelConfigurationBuilder>());
            config1.DidNotReceive().OnModelCreating(Arg.Any<ModelConfigurationBuilder>());
        }

        [Fact]
        public void ThrowExceptionWhenAddingDatabaseWithTheSameName()
        {
            var services = new ServiceCollection();
            services.AddKorm("server=localhost-1", "db1");

            Action action = () => services.AddKorm("server=localhost-2", "db1");

            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [Fact]
        public void ThrowExceptionWhenPassingInvalidNameToDatabaseFactory()
        {
            var services = new ServiceCollection();
            services.AddKorm("server=localhost-1", "db1");
            services.AddKorm("server=localhost-2", "db2");
            ServiceProvider provider = services.BuildServiceProvider();
            IDatabaseFactory factory = provider.GetService<IDatabaseFactory>();

            Action action = () => factory.GetDatabase("NonExistingName");

            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [Fact]
        public void ThrowExceptionWhenUsingDisposedDatabaseFactory()
        {
            var services = new ServiceCollection();
            services.AddKorm("server=localhost-1", "db1");
            ServiceProvider provider = services.BuildServiceProvider();
            IDatabaseFactory factory = null;
            using (IServiceScope scope = provider.CreateScope())
            {
                factory = scope.ServiceProvider.GetService<IDatabaseFactory>();
            }

            Action action = () => factory.GetDatabase("db1");

            action.Should().Throw<ObjectDisposedException>();
        }
    }
}
