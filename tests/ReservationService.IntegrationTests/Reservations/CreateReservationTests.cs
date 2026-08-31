using Dapper;
using Microsoft.Extensions.DependencyInjection;
using ReservationService.Endpoints.Reservation.Create;
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
                Guid.Parse("00000000-0000-0000-0000-000000000009"),
                Guid.Parse("00000000-0000-0000-0000-000000000021"),
                time.AddHours(2),
                time.AddHours(4))
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
}
