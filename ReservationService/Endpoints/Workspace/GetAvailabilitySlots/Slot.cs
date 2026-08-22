namespace ReservationService.Endpoints.Workspace.GetAvailabilitySlots;

public record Slot(
    DateTime StartAt,
    DateTime EndAt
);
