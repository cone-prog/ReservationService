using FluentValidation;
using FluentValidation.Results;
using Moq;
using ReservationService.Endpoints.Reservation.Confirm;
using ReservationService.Endpoints.Confirm;
using ReservationService.Exceptions;
using ReservationService.Infrastructure.Repositories;
using ReservationService.Models;

namespace ReservationService.UnitTests.Reservations;

public class ConfirmReservationHandlerTests
{
    [Fact]
    public async Task ConfirmReservationAsync_WhenRequestInvalid_ShouldThrowValidationException()
    {
        var request = new ConfirmReservationRequest(
            Guid.Empty);

        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<ConfirmReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("", "")]
            ));

        var handler = new ConfirmReservationHandler(
            reservationRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.ConfirmReservationAsync(
                request,
                CancellationToken.None
            )
        );

        reservationRepository.Verify(r => r.ConfirmReservationAsync(
            request.Id,
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task ConfirmReservationAsync_WhenReservationDoesNotExist_ShouldThrowNotFoundException()
    {
        var request = new ConfirmReservationRequest(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<ConfirmReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        reservationRepository
            .Setup(r => r.ConfirmReservationAsync(
                request.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(ConfirmReservationResult.NotFound());

        var handler = new ConfirmReservationHandler(
            reservationRepository.Object,
            validator.Object);


        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.ConfirmReservationAsync(
                request,
                CancellationToken.None
            )
        );

        reservationRepository.Verify(r => r.ConfirmReservationAsync(
            request.Id,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Theory]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.Expired)]
    [InlineData(ReservationStatus.Cancelled)]
    public async Task ConfirmReservationAsync_WhenReservationStatusIsNotPending_ShouldThrowForbiddenChangeStatusException(
        ReservationStatus status)
    {
        var request = new ConfirmReservationRequest(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

        var time = DateTime.UtcNow;
        var start = time.AddHours(2);
        var end = time.AddHours(4);

        var reservation = new Reservation()
        {
            Id = request.Id,
            WorkspaceId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            UserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
            StartAt = start,
            EndAt = end,
            Status = status,
            CreatedAt = time,
        };

        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<ConfirmReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());


        reservationRepository
            .Setup(r => r.ConfirmReservationAsync(
                reservation.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(ConfirmReservationResult.InvalidStatusChange(reservation.Status));

        var handler = new ConfirmReservationHandler(
            reservationRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<ForbiddenChangeStatusException>(
            () => handler.ConfirmReservationAsync(
                request,
                CancellationToken.None
            )
        );

        reservationRepository.Verify(r => r.ConfirmReservationAsync(
            request.Id,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ConfirmReservationAsync_WhenReservationIsPending_ShouldConfirmReservation()
    {
        var request = new ConfirmReservationRequest(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

        var time = DateTime.UtcNow;
        var start = time.AddHours(2);
        var end = time.AddHours(4);

        var reservation = new Reservation()
        {
            Id = request.Id,
            WorkspaceId = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
            UserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
            StartAt = start,
            EndAt = end,
            Status = ReservationStatus.Pending,
            CreatedAt = time,
        };

        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<ConfirmReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        reservationRepository
            .Setup(r => r.ConfirmReservationAsync(
                reservation.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(ConfirmReservationResult.Success());

        var handler = new ConfirmReservationHandler(
            reservationRepository.Object,
            validator.Object);

        await handler.ConfirmReservationAsync(request,
            CancellationToken.None);

        reservationRepository.Verify(r => r.ConfirmReservationAsync(
            request.Id,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
