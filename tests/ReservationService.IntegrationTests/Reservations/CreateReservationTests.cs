using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Exceptions;
using ReservationService.Infrastructure.Context;
using ReservationService.IntegrationTests.Fixtures;
using ReservationService.Models;

namespace ReservationService.IntegrationTests.Reservations;

public class CreateReservationTests(DatabaseFixture databaseFixture)
    : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CreateReservation_WhenValid_ShouldSaveReservation()
    {
        using var factory = new CustomWebApplicationFactory(
            databaseFixture.ConnectionString);

        await using var scope = factory.Services.CreateAsyncScope();

        var handler = scope.ServiceProvider
            .GetRequiredService<CreateReservationHandler>();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<DapperContext>();

        using var dbConnection = dbContext.CreateConnection();

        var time = DateTimeOffset.UtcNow;
        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Parse("00000000-0000-0000-0000-000000000006"),
                Guid.Parse("00000000-0000-0000-0000-000000000021"),
                time.AddHours(11),
                time.AddHours(12))
        );

        await handler.CreateReservationAsync(request,
            CancellationToken.None);

        var query = """
            SELECT *
            FROM Reservation
            WHERE WorkspaceId = @workspaceId
            AND UserId = @userId
            AND StartAt = @startAt
        """;

        var result = await dbConnection
            .QuerySingleOrDefaultAsync<Reservation>(new CommandDefinition(
                commandText: query,
                parameters: new
                {
                    workspaceId = request.Dto.WorkspaceId,
                    userId = request.Dto.UserId,
                    startAt = request.Dto.StartAt.UtcDateTime
                }
            ));

        Assert.NotNull(result);
        Assert.Equal(request.Dto.WorkspaceId, result.WorkspaceId);
        Assert.Equal(request.Dto.UserId, result.UserId);
        Assert.Equal(ReservationStatus.Pending, result.Status);
    }

    [Fact]
    public async Task CreateReservation_WhenConcurrentRequests_ShouldCreateOnlyOneReservation()
    {
        using var factory = new CustomWebApplicationFactory(
            databaseFixture.ConnectionString);

        var time = DateTimeOffset.UtcNow;

        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Parse("00000000-0000-0000-0000-000000000006"),
                Guid.Parse("00000000-0000-0000-0000-000000000021"),
                time.AddHours(13),
                time.AddHours(14))
        );

        async Task<bool> CreateAsync()
        {
            await using var scope = factory.Services.CreateAsyncScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<CreateReservationHandler>();

            return await handler.CreateReservationAsync(
                request,
                CancellationToken.None);
        }

        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DapperContext>();
        using var dbConnection = dbContext.CreateConnection();
        var tasks = new List<Task<bool>>()
        {
            CreateAsync(),
            CreateAsync(),
            CreateAsync()
        };

        var countSuccess = 0;
        var countNoSuccess = 0;
        try
        {
            await Task.WhenAll(tasks);
        }

        catch
        {
            foreach (var task in tasks)
            {
                if (task.IsFaulted)
                {
                    var exceptions = task.Exception.InnerExceptions;
                    foreach (var e in exceptions)
                    {
                        if (e is SlotNotAvailableException slotNotAvailable)
                        {
                            countNoSuccess++;
                        }
                    }
                }
                else countSuccess++;
            }
        }

        Assert.Equal(1, countSuccess);
        Assert.Equal(2, countNoSuccess);

        var query = """
            SELECT COUNT(1)
            FROM Reservation
            WHERE WorkspaceId = @workspaceId
            AND UserId = @userId
            AND StartAt = @startAt
            AND EndAt = @endAt
        """;

        var count = await dbConnection
            .ExecuteScalarAsync<int>(new CommandDefinition(
                commandText: query,
                parameters: new
                {
                    workspaceId = request.Dto.WorkspaceId,
                    userId = request.Dto.UserId,
                    startAt = request.Dto.StartAt.UtcDateTime,
                    endAt = request.Dto.EndAt.UtcDateTime
                }
            ));
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ReservationExclusionConstraint_WhenConcurrentInsertsOverlap_ShouldAllowOnlyOneReservation()
    {
        using var factory = new CustomWebApplicationFactory(
            databaseFixture.ConnectionString);

        var time = DateTimeOffset.UtcNow;

        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Parse("00000000-0000-0000-0000-000000000006"),
                Guid.Parse("00000000-0000-0000-0000-000000000021"),
                time.AddHours(15),
                time.AddHours(16))
        );

        async Task CreateAsync()
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DapperContext>();
            using var dbConnection = dbContext.CreateConnection();
            var queryCreate = """
                INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
                VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
            """;

            await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: queryCreate,
            parameters: new
            {
                id = Guid.NewGuid(),
                request.Dto.WorkspaceId,
                request.Dto.UserId,
                startAt = request.Dto.StartAt.UtcDateTime,
                endAt = request.Dto.EndAt.UtcDateTime,
                status = ReservationStatus.Pending.ToString(),
                createdAt = DateTime.UtcNow
            }
        ));

        }

        var tasks = new List<Task>()
        {
            CreateAsync(),
            CreateAsync(),
            CreateAsync()
        };

        var countSuccess = 0;
        var countNoSuccess = 0;
        try
        {
            await Task.WhenAll(tasks);
        }

        catch
        {
            foreach (var task in tasks)
            {
                if (task.IsFaulted)
                {
                    var exceptions = task.Exception.InnerExceptions;
                    foreach (var e in exceptions)
                    {
                        if (e is PostgresException postgresException)
                        {
                            if (postgresException.SqlState == "23P01")
                                countNoSuccess++;
                        }
                    }
                }
                else countSuccess++;
            }
        }


        Assert.Equal(1, countSuccess);
        Assert.Equal(2, countNoSuccess);

        var query = """
            SELECT COUNT(1)
            FROM Reservation
            WHERE WorkspaceId = @workspaceId
            AND UserId = @userId
            AND StartAt = @startAt
            AND EndAt = @endAt
        """;
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DapperContext>();
        using var dbConnection = dbContext.CreateConnection();
        var count = await dbConnection
            .ExecuteScalarAsync<int>(new CommandDefinition(
                commandText: query,
                parameters: new
                {
                    workspaceId = request.Dto.WorkspaceId,
                    userId = request.Dto.UserId,
                    startAt = request.Dto.StartAt.UtcDateTime,
                    endAt = request.Dto.EndAt.UtcDateTime
                }
            ));
        Assert.Equal(1, count);
    }
}
