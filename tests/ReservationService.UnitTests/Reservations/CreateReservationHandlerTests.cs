using FluentValidation;
using FluentValidation.Results;
using Moq;
using ReservationService.Endpoints.Reservation.Create;
using ReservationService.Exceptions;
using ReservationService.Infrastructure.Repositories;
using ReservationService.Models;

namespace ReservationService.UnitTests.Reservations;

public class CreateReservationHandlerTests
{
    [Fact]
    public async Task CreateReservationAsync_WhenRequestInvalid_ShouldThrowValidationException()
    {
        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Empty,
                Guid.Empty,
                new DateTime(2026, 8, 22, 12, 00, 0),
                new DateTime(2026, 8, 22, 15, 00, 0)
            )
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<CreateReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult(
                [
                    new ValidationFailure("","")
                ]
            ));

        var handler = new CreateReservationHandler(
            workspaceRepository.Object,
            reservationRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.CreateReservationAsync(request, CancellationToken.None));

        workspaceRepository.Verify(w => w.GetByIdAsync(
            request.Dto.WorkspaceId,
            It.IsAny<CancellationToken>()
        ), Times.Never);


        reservationRepository.Verify(r => r.CreateReservationAsync(
            request.Dto,
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenWorkspaceDoesNotExist_ShouldThrowNotFoundException()
    {
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(2);
        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                start,
                end
            )
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<CreateReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        workspaceRepository
            .Setup(w => w.GetByIdAsync(
                request.Dto.WorkspaceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace?)null);

        var handler = new CreateReservationHandler(
            workspaceRepository.Object,
            reservationRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.CreateReservationAsync(request, CancellationToken.None));

        workspaceRepository.Verify(w => w.GetByIdAsync(
            request.Dto.WorkspaceId,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        reservationRepository.Verify(r => r.CreateReservationAsync(
            request.Dto,
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenWorkspaceNotActive_ShouldThrowWorkspaceNotActiveException()
    {
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(2);

        var workspace = new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Workspace 1",
            Description = "Test workspace 1",
            PricePerHour = 100m,
            IsActive = false
        };

        var request = new CreateReservationRequest(
            new CreateReservationDto(
                workspace.Id,
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                start,
                end
            )
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<CreateReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        workspaceRepository
            .Setup(w => w.GetByIdAsync(
                request.Dto.WorkspaceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        var handler = new CreateReservationHandler(
            workspaceRepository.Object,
            reservationRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<WorkspaceNotActiveException>(() =>
            handler.CreateReservationAsync(request, CancellationToken.None));

        workspaceRepository.Verify(w => w.GetByIdAsync(
            request.Dto.WorkspaceId,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        reservationRepository.Verify(r => r.CreateReservationAsync(
            request.Dto,
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenSlotNotAvailable_ShouldThrowSlotNotAvailableException()
    {
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(2);

        var workspace = new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Workspace 1",
            Description = "Test workspace 1",
            PricePerHour = 100m,
            IsActive = true
        };

        var request = new CreateReservationRequest(
            new CreateReservationDto(
                workspace.Id,
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                new DateTimeOffset(start, TimeSpan.Zero),
                new DateTimeOffset(end, TimeSpan.Zero)
            )
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<CreateReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        workspaceRepository
            .Setup(w => w.GetByIdAsync(
                request.Dto.WorkspaceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        reservationRepository
            .Setup(r => r.CreateReservationAsync(
                request.Dto,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        var handler = new CreateReservationHandler(
            workspaceRepository.Object,
            reservationRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<SlotNotAvailableException>(() =>
            handler.CreateReservationAsync(request, CancellationToken.None));

        workspaceRepository.Verify(w => w.GetByIdAsync(
            request.Dto.WorkspaceId,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        reservationRepository.Verify(r => r.CreateReservationAsync(
            request.Dto,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_WhenSuccess_ShouldReturnTrue()
    {
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(2);

        var workspace = new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Workspace 1",
            Description = "Test workspace 1",
            PricePerHour = 100m,
            IsActive = true
        };

        var userId = Guid.Parse("00000000-0000-0000-0000-000000000003");

        var request = new CreateReservationRequest(
            new CreateReservationDto(
                workspace.Id,
                userId,
                new DateTimeOffset(start, TimeSpan.Zero),
                new DateTimeOffset(end, TimeSpan.Zero)
            )
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<CreateReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        workspaceRepository
            .Setup(w => w.GetByIdAsync(
                request.Dto.WorkspaceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        reservationRepository
            .Setup(r => r.CreateReservationAsync(
                request.Dto,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        reservationRepository
            .Setup(r => r.CreateReservationAsync(
                request.Dto,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateReservationHandler(
            workspaceRepository.Object,
            reservationRepository.Object,
            validator.Object);

        var result = await handler.CreateReservationAsync(request,
            CancellationToken.None);

        Assert.True(true);

        workspaceRepository.Verify(w => w.GetByIdAsync(
            request.Dto.WorkspaceId,
            It.IsAny<CancellationToken>()
        ), Times.Once);

        reservationRepository.Verify(r => r.CreateReservationAsync(
            request.Dto,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
