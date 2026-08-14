namespace ReservationService.Models;

public class Workspace
{
    public Guid Id { get; init; }

    public string Name { get; set; } = default!;

    public string Description { get; set; } = default!;

    public decimal PricePerHour { get; set; }

    public bool IsActive { get; set; }
}
