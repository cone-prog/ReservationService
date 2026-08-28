namespace ReservationService.Endpoints.Reservation.Confirm;

public class ConfirmReservationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reservations/{id}/confirm", async (Guid id,
            ConfirmReservationHandler handler, CancellationToken cancellationToken) =>
        {
            var request = new ConfirmReservationRequest(id);
            await handler.ConfirmReservationAsync(request, cancellationToken);
            return TypedResults.Ok();
        });
    }
}
