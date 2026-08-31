using Dapper;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Infrastructure.Context;
using ReservationService.IntegrationTests.Fixtures;
using ReservationService.Models;

namespace ReservationService.IntegrationTests.Reservations;

public class ExpireReservationTests(DatabaseFixture databaseFixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task AutoExpireReservationService_WhenReservationIsOlderThanTenMinutes_ShouldSetStatusExpired()
    {
        using var factory = new CustomWebApplicationFactory(
            databaseFixture.ConnectionString);

        await using var scope = factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<DapperContext>();

        using var dbConnection = dbContext.CreateConnection();

        var time = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = id,
            WorkspaceId = Guid.Parse("00000000-0000-0000-0000-000000000009"),
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
            StartAt = time.AddHours(1),
            EndAt = time.AddHours(2),
            Status = ReservationStatus.Pending,
            CreatedAt = time.AddMinutes(-11)
        };

        var query = """
            INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
            VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
        """;

        await dbConnection
            .ExecuteAsync(new CommandDefinition(
                commandText: query,
                parameters: new
                {
                    id = reservation.Id,
                    workspaceId = reservation.WorkspaceId,
                    userId = reservation.UserId,
                    startAt = reservation.StartAt,
                    endAt = reservation.EndAt,
                    status = reservation.Status.ToString(),
                    createdAt = reservation.CreatedAt
                }));

        var getQuery = """
            SELECT *
            FROM Reservation
            WHERE Id = @id
        """;


        Reservation? result = null;
        var timeoutAt = DateTime.UtcNow.AddSeconds(10);

        while (DateTime.UtcNow < timeoutAt)
        {
            result = await dbConnection
                .QuerySingleOrDefaultAsync<Reservation>(
                    new CommandDefinition(
                        commandText: getQuery,
                        parameters: new { id }));

            if (result?.Status == ReservationStatus.Expired)
                break;

            await Task.Delay(200, CancellationToken.None);
        }

        Assert.NotNull(result);
        Assert.Equal(ReservationStatus.Expired, result.Status);
    }

    [Fact]
    public async Task AutoExpireReservationService_WhenReservationCreatedLessThanTenMinutesAgo_ShouldStatusRemainPending()
    {
        using var factory = new CustomWebApplicationFactory(
            databaseFixture.ConnectionString);

        await using var scope = factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<DapperContext>();

        using var dbConnection = dbContext.CreateConnection();

        var time = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = id,
            WorkspaceId = Guid.Parse("00000000-0000-0000-0000-000000000009"),
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
            StartAt = time.AddHours(1),
            EndAt = time.AddHours(2),
            Status = ReservationStatus.Pending,
            CreatedAt = time.AddMinutes(-5)
        };

        var query = """
            INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
            VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
        """;

        await dbConnection
            .ExecuteAsync(new CommandDefinition(
                commandText: query,
                parameters: new
                {
                    id = reservation.Id,
                    workspaceId = reservation.WorkspaceId,
                    userId = reservation.UserId,
                    startAt = reservation.StartAt,
                    endAt = reservation.EndAt,
                    status = reservation.Status.ToString(),
                    createdAt = reservation.CreatedAt
                }));

        var getQuery = """
            SELECT *
            FROM Reservation
            WHERE Id = @id
        """;


        Reservation? result = null;
        var timeoutAt = DateTime.UtcNow.AddSeconds(10);

        while (DateTime.UtcNow < timeoutAt)
        {
            result = await dbConnection
                .QuerySingleOrDefaultAsync<Reservation>(
                    new CommandDefinition(
                        commandText: getQuery,
                        parameters: new { id }));

            if (result?.Status == ReservationStatus.Expired)
                break;

            await Task.Delay(200, CancellationToken.None);
        }

        Assert.NotNull(result);
        Assert.Equal(ReservationStatus.Pending, result.Status);
    }

    [Fact]
    public async Task AutoExpireReservationService_WhenUserConfirmedReservation_ShouldDontSetStatusExpire()
    {
        using var factory = new CustomWebApplicationFactory(
            databaseFixture.ConnectionString);

        await using var scope = factory.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<DapperContext>();

        using var dbConnection = dbContext.CreateConnection();

        var time = DateTime.UtcNow;
        var id = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = id,
            WorkspaceId = Guid.Parse("00000000-0000-0000-0000-000000000009"),
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000021"),
            StartAt = time.AddHours(1),
            EndAt = time.AddHours(2),
            Status = ReservationStatus.Confirmed,
            CreatedAt = time.AddMinutes(-11)
        };

        var query = """
            INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
            VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
        """;

        await dbConnection
            .ExecuteAsync(new CommandDefinition(
                commandText: query,
                parameters: new
                {
                    id = reservation.Id,
                    workspaceId = reservation.WorkspaceId,
                    userId = reservation.UserId,
                    startAt = reservation.StartAt,
                    endAt = reservation.EndAt,
                    status = reservation.Status.ToString(),
                    createdAt = reservation.CreatedAt
                }));

        var getQuery = """
            SELECT *
            FROM Reservation
            WHERE Id = @id
        """;


        Reservation? result = null;
        var timeoutAt = DateTime.UtcNow.AddSeconds(10);

        while (DateTime.UtcNow < timeoutAt)
        {
            result = await dbConnection
                .QuerySingleOrDefaultAsync<Reservation>(
                    new CommandDefinition(
                        commandText: getQuery,
                        parameters: new { id }));

            if (result?.Status == ReservationStatus.Expired)
                break;

            await Task.Delay(200, CancellationToken.None);
        }

        Assert.NotNull(result);
        Assert.Equal(ReservationStatus.Confirmed, result.Status);
    }
}
