using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Infrastructure.Context;
using ReservationService.Infrastructure.Repositories;
using ReservationService.IntegrationTests.Fixtures;
using ReservationService.Models;

namespace ReservationService.IntegrationTests.Reservations;

public class CreateReservationWithCache(DatabaseAndRedisFixture fixture)
    : IClassFixture<DatabaseAndRedisFixture>
{
    [Fact]
    public async Task CreateReservation_WhenRedisContainsThisDate_ShouldRedisRemovesKey()
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

        var oldSlot = new Slot(new DateTime(date, new TimeOnly(14, 0), DateTimeKind.Utc),
                new DateTime(date, new TimeOnly(16, 0), DateTimeKind.Utc));

        var newSlot = new Slot(new DateTime(date, new TimeOnly(17, 0), DateTimeKind.Utc),
                new DateTime(date, new TimeOnly(18, 0), DateTimeKind.Utc));


        var oldReservation = new Reservation()
        {
            Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440002"),
            WorkspaceId = workspaceId,
            UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440003"),
            StartAt = oldSlot.StartAt,
            EndAt = oldSlot.EndAt,
            Status = ReservationStatus.Confirmed,
            CreatedAt = new DateTime(date, new TimeOnly(10, 0), DateTimeKind.Utc)
        };

        var reservationDto = new CreateReservationDto(
            WorkspaceId: workspaceId,
            UserId: Guid.Parse("350e8400-e29b-41d4-a716-446655440005"),
            StartAt: newSlot.StartAt,
            EndAt: newSlot.EndAt
        );

        await distributedCache.SetAsync(key,
        JsonSerializer.SerializeToUtf8Bytes(new List<Slot>() { oldSlot }), CancellationToken.None);

        var query = """
            INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
            VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
        """;

        await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                oldReservation.Id,
                oldReservation.WorkspaceId,
                oldReservation.UserId,
                oldReservation.StartAt,
                oldReservation.EndAt,
                status = oldReservation.Status.ToString(),
                oldReservation.CreatedAt,
            }
        ));

        var repository = scope.ServiceProvider
            .GetRequiredService<IReservationRepository>();

        var oldCacheResult = JsonSerializer.Deserialize<List<Slot>>(
            await distributedCache.GetAsync(key, CancellationToken.None));
        var result = await repository.CreateReservationAsync(reservationDto,
            CancellationToken.None);
        var newCacheResult = await distributedCache.GetAsync(key, CancellationToken.None);

        Assert.True(result);
        Assert.NotNull(oldCacheResult);
        var slot = Assert.Single(oldCacheResult);
        Assert.Equal(oldSlot, slot);
        Assert.Null(newCacheResult);
    }
}
