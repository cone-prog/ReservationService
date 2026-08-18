using Dapper;
using ReservationService.Endpoints.Workspace;
using ReservationService.Infrastructure.Context;
using ReservationService.Models;

namespace ReservationService.Infrastructure.Repositories;

public class WorkspaceRepository(DapperContext dbContext) : IWorkspaceRepository
{
    public async Task<List<Workspace>> GetAsync(FilterAndPaginationDto filterAndPaginationDto,
        CancellationToken cancellationToken)
    {
        var skip = (filterAndPaginationDto.PageNumber - 1) * filterAndPaginationDto.PageSize;
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
                Name = filterAndPaginationDto.Name is null
                    ? null
                    : "%" + filterAndPaginationDto.Name + "%",
                Skip = skip,
                filterAndPaginationDto.IsActive,
                filterAndPaginationDto.PageSize
            },
            cancellationToken: cancellationToken
        ))).ToList();
    }

    public async Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = $"""
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
}
