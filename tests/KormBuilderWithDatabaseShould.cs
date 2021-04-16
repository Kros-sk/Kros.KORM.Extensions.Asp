using FluentAssertions;
using Kros.Data;
using Kros.KORM.Extensions.Asp;
using Kros.UnitTests;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class KormBuilderWithDatabaseShould : SqlServerDatabaseTestBase
    {
        private string _connectionString;

        protected override string BaseConnectionString
        {
            get
            {
                if (_connectionString is null)
                {
                    IConfigurationRoot configuration = ConfigurationHelper.GetConfiguration();
                    _connectionString = configuration.GetConnectionString("IdGenerator");
                }
                return _connectionString;
            }
        }

        [Fact]
        public void InitDatabaseForIdGenerators()
        {
            var kormBuilder = new KormBuilder(new ServiceCollection(), ServerHelper.Connection.ConnectionString);
            kormBuilder.InitDatabaseForIdGenerators();

            CheckTableAndProcedure();
        }

        private void CheckTableAndProcedure()
        {
            using (ConnectionHelper.OpenConnection(ServerHelper.Connection))
            using (SqlCommand cmd = ServerHelper.Connection.CreateCommand())
            {
                cmd.CommandText = "SELECT Count(*) FROM sys.tables WHERE name = 'IdStore' AND type = 'U'";
                ((int)cmd.ExecuteScalar()).Should().Be(1);

                cmd.CommandText = "SELECT Count(*) FROM sys.procedures WHERE name = 'spGetNewId' AND type = 'P'";
                ((int)cmd.ExecuteScalar()).Should().Be(1);

                cmd.CommandText = "SELECT Count(*) FROM sys.tables WHERE name = 'IdStoreInt64' AND type = 'U'";
                ((int)cmd.ExecuteScalar()).Should().Be(1);

                cmd.CommandText = "SELECT Count(*) FROM sys.procedures WHERE name = 'spGetNewIdInt64' AND type = 'P'";
                ((int)cmd.ExecuteScalar()).Should().Be(1);
            }
        }
    }
}
