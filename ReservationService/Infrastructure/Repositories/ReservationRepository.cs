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

    public async Task<bool> CreateReservationAsync(CreateReservationDto dto,
        CancellationToken cancellationToken)
    {
        var activeReservationStatuses = new[]
            {
                ReservationStatus.Confirmed.ToString(),
                ReservationStatus.Pending.ToString()
            };
        var queryLock = """
            SELECT Id FROM Workspace
            WHERE Id = @id
            FOR UPDATE
        """;

        var queryCheck = """
            SELECT NOT EXISTS(
            SELECT 1
            FROM Reservation
            WHERE WorkspaceId = @workspaceId
            AND Status = ANY(@activeReservationStatuses)
            AND @endAt > StartAt AND EndAt > @startAt 
            )
        """;

        var queryCreate = """
            INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
            VALUES (@id, @workspaceId, @userId, @startAt, @endAt, @status, @createdAt)
        """;

        using var dbConnection = dbContext.CreateConnection();
        await dbConnection.OpenAsync(cancellationToken);
        using var transaction = await dbConnection.BeginTransactionAsync();

        await dbConnection.QuerySingleAsync<Guid>(new CommandDefinition(
            commandText: queryLock,
            parameters: new { id = dto.WorkspaceId },
            cancellationToken: cancellationToken,
            transaction: transaction
        ));

        var isAvailable = await dbConnection.ExecuteScalarAsync<bool>(new CommandDefinition(
            commandText: queryCheck,
            parameters: new
            {
                dto.WorkspaceId,
                activeReservationStatuses,
                startAt = dto.StartAt.UtcDateTime,
                endAt = dto.EndAt.UtcDateTime
            },
            cancellationToken: cancellationToken,
            transaction: transaction
        ));

        if (!isAvailable)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        var result = await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: queryCreate,
            parameters: new
            {
                id = Guid.NewGuid(),
                dto.WorkspaceId,
                dto.UserId,
                startAt = dto.StartAt.UtcDateTime,
                endAt = dto.EndAt.UtcDateTime,
                status = ReservationStatus.Pending.ToString(),
                createdAt = DateTime.UtcNow
            },
            cancellationToken: cancellationToken,
            transaction: transaction
        ));

        if (result != 1)
            throw new InvalidOperationException(
            "Reservation was not created.");

        await transaction.CommitAsync(cancellationToken);
        return true;
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

    public async Task ExpirePendingReservationsAsync(CancellationToken cancellationToken)
    {
        var query = """
            UPDATE Reservation
                SET Status = 'Expired'
                WHERE Status = 'Pending'
                AND CreatedAt < @expiresBefore
        """;

        using var dbConnection = dbContext.CreateConnection();
        await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                expiresBefore = DateTime.UtcNow.AddMinutes(-10)
            },
            cancellationToken: cancellationToken
        ));
    }
}
