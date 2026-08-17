using System.Data;
using Dapper;
using Npgsql;

namespace ReservationService.Infrastructure.Extensions;

public static class Initial
{
    public static async Task InitialDataBaseAsync(IConfiguration configuration, CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("PgConnection");

        using NpgsqlConnection dbConnection = new NpgsqlConnection(connectionString);
        if (dbConnection.State != ConnectionState.Open)
        {
            await dbConnection.OpenAsync(cancellationToken);
        }

        var workspaceQuery = """
                CREATE TABLE IF NOT EXISTS Workspace (
                Id UUID NOT NULL PRIMARY KEY,
                Name VARCHAR(30) NOT NULL,
                Description TEXT NOT NULL,
                PricePerHour DECIMAL(7, 2) NOT NULL,
                IsActive BOOLEAN NOT NULL
                )
            """;
        await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: workspaceQuery,
            cancellationToken: cancellationToken));

        var reservationQuery = """
                CREATE TABLE IF NOT EXISTS Reservation (
                Id UUID NOT NULL PRIMARY KEY,
                WorkspaceId UUID NOT NULL,
                UserId UUID NOT NULL,
                StartAt TIMESTAMPTZ NOT NULL,
                EndAt TIMESTAMPTZ NOT NULL,
                Status TEXT NOT NULL,
                CreatedAt TIMESTAMPTZ NOT NULL,
                FOREIGN KEY (WorkspaceId) REFERENCES Workspace (Id)
                )
            """;

        await dbConnection.ExecuteAsync(new CommandDefinition(
            commandText: reservationQuery,
            cancellationToken: cancellationToken));

        var workspaceCount = await dbConnection.ExecuteScalarAsync<int>(new CommandDefinition(
            commandText: "SELECT COUNT(1) FROM Workspace",
            cancellationToken: cancellationToken));

        var reservationCount = await dbConnection.ExecuteScalarAsync<int>(new CommandDefinition(
            commandText: "SELECT COUNT(1) FROM Reservation",
            cancellationToken: cancellationToken));


        using var transaction = await dbConnection.BeginTransactionAsync(cancellationToken);
        try
        {
            if (workspaceCount == 0 && reservationCount == 0)
            {

                foreach (var ws in InitialData.Workspaces)
                {
                    var seedWorkspaces = """
                                INSERT INTO Workspace (Id, Name, Description, PricePerHour, IsActive)
                                VALUES (@Id, @Name, @Description, @PricePerHour, @IsActive)
                            """;
                    await dbConnection.ExecuteAsync(new CommandDefinition(
                        transaction: transaction,
                        commandText: seedWorkspaces,
                        parameters: new
                        {
                            ws.Id,
                            ws.Name,
                            ws.Description,
                            ws.PricePerHour,
                            ws.IsActive
                        },
                        cancellationToken: cancellationToken
                    ));
                }

                foreach (var rv in InitialData.Reservations)
                {
                    var seedReservations = """
                                INSERT INTO Reservation (Id, WorkspaceId, UserId, StartAt, EndAt, Status, CreatedAt)
                                VALUES (@Id, @WorkspaceId, @UserId, @StartAt, @EndAt, @Status, @CreatedAt)
                            """;
                    var status = rv.Status.ToString();
                    await dbConnection.ExecuteAsync(new CommandDefinition(
                        transaction: transaction,
                        commandText: seedReservations,
                        parameters: new
                        {
                            rv.Id,
                            rv.WorkspaceId,
                            rv.UserId,
                            rv.StartAt,
                            rv.EndAt,
                            status,
                            rv.CreatedAt
                        },
                        cancellationToken: cancellationToken));
                }
            }
            await transaction.CommitAsync(cancellationToken);
        }

        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
