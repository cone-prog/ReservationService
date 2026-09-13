namespace ReservationService.Endpoints.Reservation.Create;

public class CreateReservationEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/reservations", async ([FromBody] CreateReservationDto dto,
            CreateReservationHandler handler, CancellationToken cancellationToken) =>
        {
            var request = new CreateReservationRequest(dto);
            var result = await handler.CreateReservationAsync(request,
                cancellationToken);
            IResult response = result
                ? TypedResults.Created()
                : TypedResults.InternalServerError();

            return response;
        });
    }
}
