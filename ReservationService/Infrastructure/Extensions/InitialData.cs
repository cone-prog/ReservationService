using ReservationService.Models;

namespace ReservationService.Infrastructure.Extensions;

public static class InitialData
{
    public static List<Workspace> Workspaces { get; } = new()
    {
        new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
            Name = "Базовое место номер 1",
            Description = "Базовое место включает в себя: стул, стол, 3 розетки, бесплатный 5Ghz wifi",
            PricePerHour = 129.99M,
            IsActive = true
        },
        new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
            Name = "Базовое место номер 2",
            Description = "Базовое место включает в себя: стул, стол, 3 розетки, бесплатный 5Ghz wifi",
            PricePerHour = 129.99M,
            IsActive = true
        },
        new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
            Name = "Базовое место номер 3",
            Description = "Базовое место включает в себя: стул, стол, 3 розетки, бесплатный 5Ghz wifi",
            PricePerHour = 129.99M,
            IsActive = true
        },
        new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000009"),
            Name = "Базовое место номер 4",
            Description = "Базовое место включает в себя: стул, стол, 3 розетки, бесплатный 5Ghz wifi",
            PricePerHour = 129.99M,
            IsActive = true
        },
        new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            Name = "VIP место номер 1",
            Description = "VIP место включает в себя: эргономичное кресло, большой стол, 6 розеток, бесплатный 5Ghz wifi, дополнительный монитор и повышенную приватность",
            PricePerHour = 299.99M,
            IsActive = true
        }
    };

    public static List<Reservation> Reservations { get; } = new()
    {
        new Reservation()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000011"),
            WorkspaceId = Workspaces[0].Id,
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            StartAt = DateTime.UtcNow.AddHours(1),
            EndAt = DateTime.UtcNow.AddHours(2),
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        },
        new Reservation()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000012"),
            WorkspaceId = Workspaces[1].Id,
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            StartAt = DateTime.UtcNow.AddHours(1),
            EndAt = DateTime.UtcNow.AddHours(3),
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        },
        new Reservation()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000013"),
            WorkspaceId = Workspaces[2].Id,
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            StartAt = DateTime.UtcNow.AddHours(4),
            EndAt = DateTime.UtcNow.AddHours(6),
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        },
        new Reservation()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000014"),
            WorkspaceId = Workspaces[3].Id,
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(3),
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        },
        new Reservation()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000015"),
            WorkspaceId = Workspaces[4].Id,
            UserId = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            StartAt = DateTime.UtcNow.AddDays(2),
            EndAt = DateTime.UtcNow.AddDays(2).AddHours(2),
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        }
    };
}
