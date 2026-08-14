namespace ReservationService.Models;

public class Reservation
{
    public Guid Id { get; init; }

    public Guid WorkspaceId { get; init; }

    public Guid UserId { get; init; }

    public DateTime StartAt { get; init; }

    public DateTime EndAt { get; init; }

    public ReservationStatus Status { get; set; }

    public DateTime CreatedAt { get; init; }
}
