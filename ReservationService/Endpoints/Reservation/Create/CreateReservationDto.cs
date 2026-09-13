using System.ComponentModel;

namespace ReservationService.Endpoints.Reservation.Create;

public record CreateReservationDto(
    Guid WorkspaceId,
    Guid UserId,
    [property: Description(
        "Время начала. Формат: yyyy-MM-ddTHH:mm:ssZ или yyyy-MM-ddTHH:mm:ss±HH:mm")]
    DateTimeOffset StartAt,
    [property: Description(
        "Время начала. Формат: yyyy-MM-ddTHH:mm:ssZ или yyyy-MM-ddTHH:mm:ss±HH:mm")]
    DateTimeOffset EndAt
);
