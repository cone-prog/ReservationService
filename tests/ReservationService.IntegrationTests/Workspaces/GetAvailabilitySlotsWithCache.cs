using System.Text.Json;
using Dapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Infrastructure.Context;
using ReservationService.Infrastructure.Repositories;
using ReservationService.IntegrationTests.Fixtures;
using ReservationService.Models;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace ReservationService.IntegrationTests.Workspaces;

public class GetAvailabilitySlotsWithCache(DatabaseAndRedisFixture fixture)
    : IClassFixture<DatabaseAndRedisFixture>
{
    [Fact]
    public async Task GetAvailabilitySlots_WhenSlotsNotExistsRedis_ShouldAddAfterFirstRequest()
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
                Status = ReservationStatus.Confirmed,
                CreatedAt = new DateTime(date, new TimeOnly(10, 0), DateTimeKind.Utc)
            },

            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440004"),
                WorkspaceId = workspaceId,
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440005"),
                StartAt = slots[1].StartAt,
                EndAt = slots[1].EndAt,
                Status = ReservationStatus.Confirmed,
                CreatedAt = new DateTime(date, new TimeOnly(10, 0), DateTimeKind.Utc)
            },

            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440020"),
                WorkspaceId = workspaceId,
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440007"),
                StartAt = slots[2].StartAt,
                EndAt = slots[2].EndAt,
                Status = ReservationStatus.Confirmed,
                CreatedAt = new DateTime(date, new TimeOnly(10, 0), DateTimeKind.Utc)
            }
        };

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

        var key = $"availability:{workspaceId}:{date.ToString("yyyy-MM-dd")}";
        var resultBefore = await distributedCache.GetAsync(key,
            CancellationToken.None);

        var repository = scope.ServiceProvider
            .GetRequiredService<IWorkspaceRepository>();

        var result = await repository.GetAvailabilitySlotsAsync(workspaceId,
            date, CancellationToken.None);

        var resultAfter = JsonSerializer.Deserialize<List<Slot>>(await distributedCache.GetAsync(key,
            CancellationToken.None));

        Assert.Equal(slots, result);
        Assert.Null(resultBefore);
        Assert.Equal(slots, resultAfter);
    }

    [Fact]
    public async Task GetAvailabilitySlots_WhenRedisUnavailable_ShouldReturnDataFromPostgres()
    {
        await using var redisContainer = new RedisBuilder("redis:8")
            .Build();

        await redisContainer.StartAsync(CancellationToken.None);

        var redisConnectionString = redisContainer.GetConnectionString();

        await redisContainer.StopAsync(CancellationToken.None);

        await using var factory = new CustomWebApplicationFactory(
           fixture.ConnectionStringDb,
           redisConnectionString
       );

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DapperContext>();
        var distributedCache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        using var dbConnection = dbContext.CreateConnection();

        var workspaceId = Guid.Parse("00000000-0000-0000-0000-000000000007");
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
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440009"),
                WorkspaceId = workspaceId,
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440010"),
                StartAt = slots[0].StartAt,
                EndAt = slots[0].EndAt,
                Status = ReservationStatus.Confirmed,
                CreatedAt = new DateTime(date, new TimeOnly(10, 0), DateTimeKind.Utc)
            },

            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440011"),
                WorkspaceId = workspaceId,
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440012"),
                StartAt = slots[1].StartAt,
                EndAt = slots[1].EndAt,
                Status = ReservationStatus.Confirmed,
                CreatedAt = new DateTime(date, new TimeOnly(10, 0), DateTimeKind.Utc)
            },

            new Reservation(){
                Id = Guid.Parse("350e8400-e29b-41d4-a716-446655440013"),
                WorkspaceId = workspaceId,
                UserId = Guid.Parse("350e8400-e29b-41d4-a716-446655440014"),
                StartAt = slots[2].StartAt,
                EndAt = slots[2].EndAt,
                Status = ReservationStatus.Confirmed,
                CreatedAt = new DateTime(date, new TimeOnly(10, 0), DateTimeKind.Utc)
            }
        };

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

        var repository = scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>();
        var key = $"availability:{workspaceId}:{date.ToString("yyyy-MM-dd")}";

        var result = await repository.GetAvailabilitySlotsAsync(workspaceId, date,
            CancellationToken.None);

        Assert.True(slots.SequenceEqual(result));
    }
}
