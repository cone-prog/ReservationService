using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Infrastructure.Context;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public class ReservationRepository(DapperContext dbContext) : IReservationRepository
{
    public async Task<Reservation?> GetByIdAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var query = """
            SELECT * FROM Reservation WHERE Id = @Id;
        """;

        using var dbConnection = dbContext.CreateConnection();
        var result = await dbConnection.QuerySingleOrDefaultAsync<Reservation>(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                id
            },
            cancellationToken: cancellationToken
        ));
        return result;
    }

    public async Task<bool> IsSlotAvailableAsync(Guid workspaceId, DateTime start,
        DateTime end, CancellationToken cancellationToken)
    {
        var activeReservationStatuses = new[]
            {
                ReservationStatus.Confirmed.ToString(),
                ReservationStatus.Pending.ToString()
            };
        var query = """
            SELECT NOT EXISTS(
            SELECT 1
            FROM Reservation
            WHERE WorkspaceId = @workspaceId
            AND Status = ANY(@activeReservationStatuses)
            AND @endAt > StartAt AND EndAt > @startAt 
            )
        """;
        using var dbConnection = dbContext.CreateConnection();
        return await dbConnection.ExecuteScalarAsync<bool>(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                workspaceId,
                activeReservationStatuses,
                startAt = start,
                endAt = end
            },
            cancellationToken: cancellationToken
        ));
    }

    public async Task<bool> CreateReservationAsync(CreateReservationDto dto,
        CancellationToken cancellationToken)
    {
        var query = """
            INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
            VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
        """;

        using var dbConnection = dbContext.CreateConnection();
        var result = await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                id = Guid.NewGuid(),
                dto.WorkspaceId,
                dto.UserId,
                startAt = dto.StartAt.UtcDateTime,
                EndAt = dto.EndAt.UtcDateTime,
                status = ReservationStatus.Pending.ToString(),
                createdAt = DateTime.UtcNow
            },
            cancellationToken: cancellationToken
        ));
        return result > 0;
    }

    public async Task ConfirmReservationAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var query = """
            UPDATE Reservation SET Status = 'Confirmed' WHERE Id = @id
        """;

        using var dbConnection = dbContext.CreateConnection();
        await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                id,
            },
            cancellationToken: cancellationToken
        ));
    }

    public async Task CancelReservationAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = """
            UPDATE Reservation SET Status = 'Cancelled' WHERE Id = @id
        """;

        using var dbConnection = dbContext.CreateConnection();
        await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                id,
            },
            cancellationToken: cancellationToken
        ));
    }
}