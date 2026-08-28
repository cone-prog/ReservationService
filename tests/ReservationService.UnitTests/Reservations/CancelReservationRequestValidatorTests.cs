using FluentValidation.TestHelper;
using ReservationService.Endpoints.Reservation.Cancel;

namespace ReservationService.UnitTests.Reservations;

public class CancelReservationRequestValidatorTests
{
    [Fact]
    public async Task Validate_WhenIdEmpty_ShouldHaveValidationError()
    {
        var request = new CancelReservationRequest(Guid.Empty);
        var validator = new CancelReservationRequestValidator();

        var validationResult = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        validationResult.ShouldHaveValidationErrorFor(r => r.Id);
    }

    [Fact]
    public async Task Validate_WhenRequestValid_ShouldNotHaveValidationError()
    {
        var request = new CancelReservationRequest(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000")
        );
        var validator = new CancelReservationRequestValidator();

        var validationResult = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        validationResult.ShouldNotHaveAnyValidationErrors();
    }
}
