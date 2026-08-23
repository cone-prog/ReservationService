namespace ReservationService.Endpoints.Reservation.Create;

public record CreateReservationDto(
    Guid WorkspaceId,
    Guid UserId,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt
);
