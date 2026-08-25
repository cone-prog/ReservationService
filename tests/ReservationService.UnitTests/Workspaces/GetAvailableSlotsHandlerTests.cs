using FluentValidation;
using FluentValidation.Results;
using Moq;
using ReservationService.Endpoints.Workspace.GetAvailabilitySlots;
using ReservationService.Exceptions;
using ReservationService.Infrastructure.Repositories;

namespace ReservationService.UnitTests.Workspaces;

public class GetAvailableSlotsHandlerTests
{
    [Fact]
    public async Task GetAvailabilitySlotsAsync_WhenRequestInvalid_ShouldThrowValidationException()
    {
        var request = new GetAvailableSlotsRequest(
            Guid.Empty,
            DateOnly.MinValue
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var validator = new Mock<IValidator<GetAvailableSlotsRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("", "")
            ]));

        var handler = new GetAvailableSlotsHandler(
            workspaceRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.GetAvailabilitySlotsAsync(
                request, CancellationToken.None));

        workspaceRepository
            .Verify(r => r.GetAvailabilitySlotsAsync(
                request.WorkspaceId,
                request.Date,
                It.IsAny<CancellationToken>()),
            Times.Never);

        workspaceRepository
            .Verify(r => r.ExistsAsync(
                request.WorkspaceId,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAvailabilitySlotsAsync_WhenWorkspaceDoesNotExist_ShouldThrowNotFoundException()
    {
        var request = new GetAvailableSlotsRequest(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            new DateOnly(2026, 08, 25)
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var validator = new Mock<IValidator<GetAvailableSlotsRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        workspaceRepository
            .Setup(r => r.ExistsAsync(
                request.WorkspaceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new GetAvailableSlotsHandler(
            workspaceRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.GetAvailabilitySlotsAsync(
                request, CancellationToken.None));

        workspaceRepository
            .Verify(r => r.GetAvailabilitySlotsAsync(
                request.WorkspaceId,
                request.Date,
                It.IsAny<CancellationToken>()),
            Times.Never);

        workspaceRepository.Verify(
            r => r.ExistsAsync(
                request.WorkspaceId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAvailabilitySlotsAsync_WhenRequestValidAndWorkspaceExists_ShouldReturnResult()
    {
        var request = new GetAvailableSlotsRequest(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            new DateOnly(2026, 8, 25)
        );

        var slots = new List<Slot>(){
                new Slot(
                    StartAt: new DateTime(2026, 8, 25, 12, 30, 0),
                    EndAt: new DateTime(2026, 8, 25, 14, 30, 0)
                ),

                new Slot(
                    StartAt: new DateTime(2026, 8, 25, 15, 0, 0),
                    EndAt: new DateTime(2026, 8, 25, 15, 0, 0)
                )
            };

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var validator = new Mock<IValidator<GetAvailableSlotsRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        workspaceRepository
            .Setup(r => r.ExistsAsync(
                request.WorkspaceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        workspaceRepository
            .Setup(r => r.GetAvailabilitySlotsAsync(
                request.WorkspaceId,
                request.Date,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(slots);

        var handler = new GetAvailableSlotsHandler(
            workspaceRepository.Object,
            validator.Object);

        var result = await handler.GetAvailabilitySlotsAsync(
            request,
            CancellationToken.None);

        Assert.Equal(request.WorkspaceId, result.WorkspaceId);
        Assert.Equal(request.Date, result.Date);
        Assert.Equal(slots, result.OccupiedSlots);

        workspaceRepository
            .Verify(r => r.GetAvailabilitySlotsAsync(
                request.WorkspaceId,
                request.Date,
                It.IsAny<CancellationToken>()),
            Times.Once);

        workspaceRepository
            .Verify(r => r.ExistsAsync(
                request.WorkspaceId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
