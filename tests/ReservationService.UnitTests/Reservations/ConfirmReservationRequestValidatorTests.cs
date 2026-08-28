using FluentValidation.TestHelper;
using ReservationService.Endpoints.Reservation.Confirm;

namespace ReservationService.UnitTests.Reservations;

public class ConfirmReservationRequestValidatorTests
{
    [Fact]
    public async Task Validation_WhenIdEmpty_ShouldHaveValidationError()
    {
        var request = new ConfirmReservationRequest(Guid.Empty);
        var validator = new ConfirmReservationRequestValidator();

        var validationResult = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        validationResult.ShouldHaveValidationErrorFor(r => r.Id);
    }

    [Fact]
    public async Task Validation_WhenRequestValid_ShouldNotHaveValidationError()
    {
        var request = new ConfirmReservationRequest(
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000")
        );
        var validator = new ConfirmReservationRequestValidator();

        var validationResult = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        validationResult.ShouldNotHaveAnyValidationErrors();
    }
}
