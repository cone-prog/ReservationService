using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ReservationService.IntegrationTests.Fixtures;

public class DatabaseAndRedisFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgreSqlContainer;
    private readonly RedisContainer redisContainer;
    public string ConnectionStringDb => postgreSqlContainer.GetConnectionString();
    public string ConnectionStringRedis => redisContainer.GetConnectionString();


    public DatabaseAndRedisFixture()
    {
        postgreSqlContainer = new PostgreSqlBuilder("postgres:18")
            .WithDatabase("Test-db")
            .WithUsername("test")
            .WithPassword("5258")
            .Build();

        redisContainer = new RedisBuilder("redis:8")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await postgreSqlContainer.StartAsync();
        await redisContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await postgreSqlContainer.DisposeAsync();
        await redisContainer.DisposeAsync();
    }
}
