using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Infrastructure.Context;
using ReservationService.Infrastructure.Repositories;
using ReservationService.IntegrationTests.Fixtures;
using ReservationService.Models;

namespace ReservationService.IntegrationTests.Reservations;

public class CancelReservationAsyncWithCache(DatabaseAndRedisFixture fixture)
    : IClassFixture<DatabaseAndRedisFixture>
{
    [Fact]
    public async Task CancelReservation_WhenRedisContainsThisDate_ShouldRedisRemovesKey()
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

        var key = $"availability:{workspaceId}:{date.ToString("yyyy-MM-dd")}";

        var slot = new Slot(new DateTime(date, new TimeOnly(14, 0), DateTimeKind.Utc),
                new DateTime(date, new TimeOnly(16, 0), DateTimeKind.Utc));

        var reservation = new Reservation()
        {
            Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440002"),
            WorkspaceId = workspaceId,
            UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440003"),
            StartAt = slot.StartAt,
            EndAt = slot.EndAt,
            Status = ReservationStatus.Confirmed,
            CreatedAt = new DateTime(date, new TimeOnly(10, 0), DateTimeKind.Utc)
        };


        await distributedCache.SetAsync(key,
        JsonSerializer.SerializeToUtf8Bytes(new List<Slot>() { slot }), CancellationToken.None);

        var query = """
            INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
            VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
        """;

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

        var repository = scope.ServiceProvider
            .GetRequiredService<IReservationRepository>();

        var oldCacheResult = JsonSerializer.Deserialize<List<Slot>>(
            await distributedCache.GetAsync(key, CancellationToken.None));

        await repository.CancelReservationAsync(reservation.Id,
            CancellationToken.None);

        var cancelledReservationStatus = await dbConnection.QuerySingleAsync<string>(
            new CommandDefinition(
                commandText: """
                    SELECT Status
                    FROM Reservation
                    WHERE Id = @id
                """,
                parameters: new
                {
                    reservation.Id
                }
        ));

        var newCacheResult = await distributedCache.GetAsync(key, CancellationToken.None);
        Assert.Equal(ReservationStatus.Cancelled.ToString(), cancelledReservationStatus);
        Assert.NotNull(oldCacheResult);
        var result = Assert.Single(oldCacheResult);
        Assert.Equal(result, slot);
        Assert.Null(newCacheResult);
    }
}

