using Kros.Data;
using Kros.KORM.Extensions.Asp;
using Kros.UnitTests;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests
{
    public class KormBuilderWithDatabaseShould : SqlServerDatabaseTestBase, IAsyncLifetime
    {
        private readonly MsSqlContainer _msSqlContainer;
        private string _connectionString;

        public KormBuilderWithDatabaseShould()
        {
            // https://testcontainers.com/modules/mssql/?language=dotnet
            _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU10-ubuntu-22.04").Build();
        }

        protected override string BaseConnectionString
        {
            get
            {
                if (_connectionString is null)
                {
                    IConfigurationRoot configuration = ConfigurationHelper.GetConfiguration();
                    _connectionString = _msSqlContainer.GetConnectionString();
                }
                return _connectionString;
            }
        }

        public async ValueTask InitializeAsync() => await _msSqlContainer.StartAsync();

        public async ValueTask DisposeAsync() => await _msSqlContainer.DisposeAsync();

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
                Assert.Equal(1, (int)cmd.ExecuteScalar());

                cmd.CommandText = "SELECT Count(*) FROM sys.procedures WHERE name = 'spGetNewId' AND type = 'P'";
                Assert.Equal(1, (int)cmd.ExecuteScalar());

                cmd.CommandText = "SELECT Count(*) FROM sys.tables WHERE name = 'IdStoreInt64' AND type = 'U'";
                Assert.Equal(1, (int)cmd.ExecuteScalar());

                cmd.CommandText = "SELECT Count(*) FROM sys.procedures WHERE name = 'spGetNewIdInt64' AND type = 'P'";
                Assert.Equal(1, (int)cmd.ExecuteScalar());
            }
        }
    }
}
