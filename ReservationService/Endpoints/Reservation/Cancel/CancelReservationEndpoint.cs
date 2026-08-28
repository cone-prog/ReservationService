namespace ReservationService.Endpoints.Reservation.Cancel;

public class CancelReservationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reservations/{id}/cancel", async (Guid id, CancelReservationHandler handler,
            CancellationToken cancellationToken) =>
        {
            var request = new CancelReservationRequest(id);

            await handler.CancelReservationAsync(request, cancellationToken);

            return TypedResults.Ok();
        });
    }
}
