using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReservationService.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory(string connectionString)
        : WebApplicationFactory<Program>
{
    private readonly string connectionString = connectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:PgConnection",
            connectionString);
    }
}
