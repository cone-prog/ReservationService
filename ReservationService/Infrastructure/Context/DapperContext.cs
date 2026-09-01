using System.Data;
using Npgsql;

namespace ReservationService.Infrastructure.Context;

public class DapperContext(IConfiguration configuration)
{
    private readonly string connectionString = configuration.GetConnectionString("PgConnection")!;

    public NpgsqlConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
