using ReservationService.Endpoints.Workspace.Get;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Infrastructure.Context;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public class WorkspaceRepository(DapperContext dbContext) : IWorkspaceRepository
{
    public async Task<List<Workspace>> GetAsync(GetWorkspacesRequest request,
        CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        var query = """
            SELECT *
            FROM Workspace
            WHERE (@Name IS NULL OR NAME ILIKE @NAME) AND (@IsActive IS NULL OR @IsActive = IsActive)
            ORDER BY Name, Id
            OFFSET @Skip
            LIMIT @PageSize
        """;

        using var dbConnection = dbContext.CreateConnection();
        return (await dbConnection.QueryAsync<Workspace>(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                Name = request.Name is null
                    ? null
                    : "%" + request.Name + "%",
                Skip = skip,
                request.IsActive,
                request.PageSize
            },
            cancellationToken: cancellationToken
        ))).ToList();
    }

    public async Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = """
            SELECT *
            FROM Workspace
            WHERE Id = @Id
        """;
        using var dbConnection = dbContext.CreateConnection();
        return await dbConnection.QuerySingleOrDefaultAsync<Workspace>(new CommandDefinition(
            commandText: query,
            parameters: new { Id = id },
            cancellationToken: cancellationToken
        ));
    }

    public async Task<List<Slot>> GetAvailabilitySlotsAsync(Guid id,
        DateOnly date, CancellationToken cancellationToken)
    {

        var query = """
            SELECT StartAt, EndAt
            FROM Reservation
            WHERE @dateStart < EndAt AND @dateEnd > StartAt AND WorkspaceId = @id
                AND Status = ANY(@Statuses)
            ORDER BY StartAt
        """;
        var dateStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dateEnd = dateStart.AddDays(1);
        using var dbConnection = dbContext.CreateConnection();
        var statuses = new string[2] { ReservationStatus.Pending.ToString(),
            ReservationStatus.Confirmed.ToString()};
        var slots = await dbConnection.QueryAsync<Slot>(new CommandDefinition(
            commandText: query,
            parameters: new
            {
                dateStart,
                dateEnd,
                id,
                statuses
            },
            cancellationToken: cancellationToken
        ));
        return slots.ToList();
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = """
            SELECT EXISTS (SELECT 1 FROM Workspace 
                           WHERE Id = @id)
        """;
        using var dbConnection = dbContext.CreateConnection();
        var result = await dbConnection.ExecuteScalarAsync<bool>(new CommandDefinition(
            commandText: query,
            parameters: new { id },
            cancellationToken: cancellationToken
        ));
        return result;
    }
}
