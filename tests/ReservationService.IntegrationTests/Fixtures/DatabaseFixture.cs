using Testcontainers.PostgreSql;

namespace ReservationService.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSqlContainer;
    public string ConnectionString => postgreSqlContainer.GetConnectionString();

    public DatabaseFixture()
    {
        postgreSqlContainer = new PostgreSqlBuilder("postgres:18")
            .WithDatabase("Test-db")
            .WithUsername("test")
            .WithPassword("5258")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await postgreSqlContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await postgreSqlContainer.DisposeAsync();
    }
}
