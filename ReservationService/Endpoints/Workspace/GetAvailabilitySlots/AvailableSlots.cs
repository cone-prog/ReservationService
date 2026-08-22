namespace ReservationService.Endpoints.Workspace.GetAvailabilitySlots;

public record AvailableSlots(
    Guid WorkspaceId,
    DateOnly Date,
    List<Slot> OccupiedSlots
);
