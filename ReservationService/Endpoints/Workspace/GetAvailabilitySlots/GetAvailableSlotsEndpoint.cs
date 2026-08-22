namespace ReservationService.Endpoints.Workspace.GetAvailabilitySlots;

public class GetAvailableSlotsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/workspaces/{id}/availability", async (Guid id, [FromQuery] DateOnly date,
            GetAvailableSlotsHandler handler, CancellationToken cancellationToken) =>
        {
            var request = new GetAvailableSlotsRequest(id, date);
            return TypedResults.Ok(await handler.GetAvailabilitySlotsAsync(request, cancellationToken));
        });
    }
}
