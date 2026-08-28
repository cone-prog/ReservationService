using FluentValidation;
using FluentValidation.Results;
using Moq;
using ReservationService.Endpoints.Reservation.Cancel;
using ReservationService.Exceptions;
using ReservationService.Infrastructure.Repositories;
using ReservationService.Models;

namespace ReservationService.UnitTests.Reservations;

public class CancelReservationHandlerTests
{
    [Fact]
    public async Task CancelReservationAsync_WhenRequestInvalid_ShouldThrowValidationException()
    {
        var request = new CancelReservationRequest(
            Guid.Empty);

        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<CancelReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("", "")]
            ));

        var handler = new CancelReservationHandler(
            reservationRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.CancelReservationAsync(
                request,
                CancellationToken.None
            )
        );

        reservationRepository.Verify(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()
        ), Times.Never);

        reservationRepository.Verify(r => r.CancelReservationAsync(
            request.Id,
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task CancelReservationAsync_WhenReservationDoesNotExist_ShouldThrowNotFoundException()
    {
        var request = new CancelReservationRequest(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));

        var reservationRepository = new Mock<IReservationRepository>();
        var validator = new Mock<IValidator<CancelReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        reservationRepository
            .Setup(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var handler = new CancelReservationHandler(
            reservationRepository.Object,
            validator.Object);


        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.CancelReservationAsync(
                request,
                CancellationToken.None
            )
        );

        reservationRepository.Verify(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()
        ), Times.Once);

        reservationRepository.Verify(r => r.CancelReservationAsync(
            request.Id,
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Theory]
    [InlineData(ReservationStatus.Expired)]
    [InlineData(ReservationStatus.Cancelled)]
    public async Task CancelReservationAsync_WhenReservationStatusIsNotPendingOrConfirmed_ShouldThrowForbiddenChangeStatusException(
        ReservationStatus status)
    {
        var request = new CancelReservationRequest(
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
        var validator = new Mock<IValidator<CancelReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        reservationRepository
            .Setup(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var handler = new CancelReservationHandler(
            reservationRepository.Object,
            validator.Object);

        await Assert.ThrowsAsync<ForbiddenChangeStatusException>(
            () => handler.CancelReservationAsync(
                request,
                CancellationToken.None
            )
        );

        reservationRepository.Verify(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()
        ), Times.Once);

        reservationRepository.Verify(r => r.CancelReservationAsync(
            request.Id,
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Theory]
    [InlineData(ReservationStatus.Pending)]
    [InlineData(ReservationStatus.Confirmed)]
    public async Task CancelReservationAsync_WhenReservationIsPendingOrConfirmed_ShouldCancelReservation(
        ReservationStatus status)
    {
        var request = new CancelReservationRequest(
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
        var validator = new Mock<IValidator<CancelReservationRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new ValidationResult());

        reservationRepository
            .Setup(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        var handler = new CancelReservationHandler(
            reservationRepository.Object,
            validator.Object);

        await handler.CancelReservationAsync(request,
            CancellationToken.None);

        reservationRepository.Verify(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()
        ), Times.Once);

        reservationRepository.Verify(r => r.CancelReservationAsync(
            request.Id,
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}

