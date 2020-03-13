using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TestContainers.Container.Abstractions;
using TestContainers.Container.Abstractions.Hosting;
using TestContainers.Container.Abstractions.Networks;
using TestContainers.Container.Database.Hosting;
using TestContainers.Container.Database.PostgreSql;

namespace LogicBuilder.Kendo.ExpressionExtensions.IntegrationTests.Emulators
{
    public class PostgresEmulator
    {
        public IContainer Container { get; }

        public PostgresEmulator(IConfiguration configuration) :
            this(configuration.GetValue<string>("Image"),
                configuration.GetValue<string>("Tag"),
                configuration.GetValue<string>("DatabaseName"),
                configuration.GetValue<string>("Username"),
                configuration.GetValue<string>("Password"),
                configuration.GetValue<int>("Port"),
                configuration.GetValue<string>("NetworkName"),
                configuration.GetValue<string>("NetworkAlias"))
        {
        }

        public PostgresEmulator(string image = "postgres",
            string tag = "11-alpine",
            string databaseName = "postgres",
            string username = "postgres",
            string password = "",
            int port = 5432,
            string networkName = null,
            string networkAlias = "postgres")
        {
            Container = new ContainerBuilder<PostgreSqlContainer>()
                .ConfigureDockerImageName($"{image}:{tag}")
                .ConfigureNetwork((h1, c) =>
                {
                    return new NetworkBuilder<UserDefinedNetwork>()
                        .WithContextFrom(c)
                        .ConfigureNetwork((h2, n) =>
                        {
                            n.NetworkName = networkName ?? n.NetworkName;
                        })
                        .Build();
                })
                .ConfigureDatabaseConfiguration(
                    databaseName: databaseName,
                    username: username,
                    password: password)
                .ConfigureContainer((h, c) =>
                {
                    c.PortBindings.Add(PostgreSqlContainer.DefaultPort, port);
                    c.NetWorkAliases.Add(networkAlias);
                })
                .Build();
        }

        public async Task InitializeAsync()
        {
            await Container.StartAsync();
        }

        public Task DisposeAsync()
        {
            return Container?.StopAsync();
        }

        public string GetConnectionString()
        {
            return ((PostgreSqlContainer) Container).GetConnectionString();
        }
    }
}
