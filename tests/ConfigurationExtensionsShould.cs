using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class ConfigurationExtensionsShould
    {
        private const string TestConnectionString = "server=servername\\instancename;initial catalog=database";
        private const string DefaultKormProvider = Kros.Data.SqlServer.SqlServerDataHelper.ClientId;
        private const bool DefaultAutoMigrate = false;

        [Theory]
        [InlineData("NoKormSettings", TestConnectionString, DefaultKormProvider, DefaultAutoMigrate)]
        [InlineData("NoConnectionString", null, "DolorSitAmet", DefaultAutoMigrate)]
        [InlineData("AutoMigrateTrue", TestConnectionString, DefaultKormProvider, true)]
        [InlineData("CustomProvider", TestConnectionString, "LoremIpsum", true)]
        [InlineData("InvalidSettings", TestConnectionString, DefaultKormProvider, DefaultAutoMigrate)]
        public void LoadCorrectKormConnectionSetings(string name, string connectionString, string kormProvider, bool autoMigrate)
        {
            IConfigurationRoot configuration = ConfigurationHelper.GetConfiguration();
            var expected = new KormConnectionSettings()
            {
                ConnectionString = connectionString,
                KormProvider = kormProvider,
                AutoMigrate = autoMigrate
            };

            KormConnectionSettings actual = configuration.GetKormConnectionString(name);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ReturnNullIfNameIsNotInEitherSection()
        {
            IConfigurationRoot configuration = ConfigurationHelper.GetConfiguration();
            configuration.GetKormConnectionString("ThisNameDoesNotExist").Should().BeNull();
        }
    }
}
