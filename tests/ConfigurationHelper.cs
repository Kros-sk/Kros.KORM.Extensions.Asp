using Microsoft.Extensions.Configuration;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    internal class ConfigurationHelper
    {
        public static IConfigurationRoot GetConfiguration()
            => new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.local.json", optional: true)
            .Build();
    }
}
