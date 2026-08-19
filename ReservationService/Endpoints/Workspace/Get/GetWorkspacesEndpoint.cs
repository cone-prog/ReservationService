namespace ReservationService.Endpoints.Workspace.Get;

public class GetWorkspacesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/workspaces", async ([AsParameters] GetWorkspacesRequest dto, GetWorkspacesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var workspaces = await handler.GetWorkspacesAsync(dto, cancellationToken);
            return TypedResults.Ok(workspaces);
        });
    }
}
