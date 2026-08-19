namespace ReservationService.Endpoints.Workspace.GetById;

public class GetWorkspaceByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/workspaces/{id}", async (Guid id, GetWorkspaceByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var request = new GetWorkspaceByIdRequest(id);
            var result = await handler.GetWorkspaceByIdAsync(request, cancellationToken);
            return Results.Ok(result);
        });
    }
}
