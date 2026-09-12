using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Infrastructure.Context;
using ReservationService.IntegrationTests.Fixtures;
using ReservationService.Models;

namespace ReservationService.IntegrationTests.Reservations;

public class ExpirePendingReservationsWithCache(DatabaseAndRedisFixture fixture)
    : IClassFixture<DatabaseAndRedisFixture>
{
    [Fact]
    public async Task ExpirePendingReservations_WhenOneReservationExpires_ShouldRemoveAllSlots()
    {
        await using var factory = new CustomWebApplicationFactory(
            fixture.ConnectionStringDb,
            fixture.ConnectionStringRedis
        );

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DapperContext>();
        var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        using var dbConnection = dbContext.CreateConnection();

        var workspaceId = Guid.Parse("00000000-0000-0000-0000-000000000006");

        var date = new DateOnly(2026, 10, 28);
        var slots = new List<Slot>()
        {
            new Slot(new DateTime(date, new TimeOnly(14, 0), DateTimeKind.Utc),
                new DateTime(date, new TimeOnly(16, 0), DateTimeKind.Utc)),

            new Slot(new DateTime(date, new TimeOnly(17, 0), DateTimeKind.Utc),
                new DateTime(date, new TimeOnly(18, 0), DateTimeKind.Utc)),

            new Slot(new DateTime(date, new TimeOnly(19, 0), DateTimeKind.Utc),
                new DateTime(date, new TimeOnly(20, 0), DateTimeKind.Utc)),
        };


        var reservations = new List<Reservation>()
        {
            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440002"),
                WorkspaceId = workspaceId,
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440003"),
                StartAt = slots[0].StartAt,
                EndAt = slots[0].EndAt,
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-11)
            },

            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440004"),
                WorkspaceId = workspaceId,
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440005"),
                StartAt = slots[1].StartAt,
                EndAt = slots[1].EndAt,
                Status = ReservationStatus.Confirmed,
                CreatedAt = DateTime.UtcNow.AddMinutes(-11)
            },

            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440020"),
                WorkspaceId = workspaceId,
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440007"),
                StartAt = slots[2].StartAt,
                EndAt = slots[2].EndAt,
                Status = ReservationStatus.Confirmed,
                CreatedAt = DateTime.UtcNow.AddMinutes(-11)
            }
        };

        var key = $"availability:{reservations[0].WorkspaceId}:{date.ToString("yyyy-MM-dd")}";

        await distributedCache.SetAsync(key,
            JsonSerializer.SerializeToUtf8Bytes(slots), CancellationToken.None);

        var query = """
            INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
            VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
        """;

        foreach (var reservation in reservations)
        {
            await dbConnection.ExecuteAsync(new CommandDefinition(
                commandText: query,
                parameters: new
                {
                    reservation.Id,
                    reservation.WorkspaceId,
                    reservation.UserId,
                    reservation.StartAt,
                    reservation.EndAt,
                    status = reservation.Status.ToString(),
                    reservation.CreatedAt,
                }
            ));
        }



        var getQuery = """
            SELECT *
            FROM Reservation
            WHERE Id = @id
        """;

        var timeoutAt = DateTime.UtcNow.AddSeconds(10);

        while (DateTime.UtcNow < timeoutAt)
        {

            var reservation = await dbConnection
                .QuerySingleOrDefaultAsync<Reservation>(
                    new CommandDefinition(
                        commandText: getQuery,
                        parameters: new { id = reservations[0].Id }));

            if (reservation?.Status == ReservationStatus.Expired)
                break;

            await Task.Delay(200, CancellationToken.None);
        }

        var timeoutAt2 = DateTime.UtcNow.AddSeconds(10);
        byte[]? result = null;
        while (DateTime.UtcNow < timeoutAt2)
        {
            result = await distributedCache.GetAsync(key, CancellationToken.None);
            if (result is null)
                break;

            await Task.Delay(200, CancellationToken.None);
        }

        Assert.Null(result);
    }
}
