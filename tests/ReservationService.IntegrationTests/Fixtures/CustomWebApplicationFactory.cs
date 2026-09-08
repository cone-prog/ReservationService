using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReservationService.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory(string connectionStringDb,
    string connectionStringRedis)
    : WebApplicationFactory<Program>
{
    private readonly string connectionStringDb = connectionStringDb;
    private readonly string connectionStringRedis = connectionStringRedis;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:PgConnection",
            connectionStringDb);
        builder.UseSetting("ConnectionStrings:RedisConnection",
            connectionStringRedis);
    }
}
