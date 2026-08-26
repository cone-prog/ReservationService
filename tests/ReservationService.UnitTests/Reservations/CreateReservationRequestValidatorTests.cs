using FluentValidation.TestHelper;
using ReservationService.Endpoints.Reservation.Create;

namespace ReservationService.UnitTests.Reservations;

public class CreateReservationRequestValidatorTests
{
    [Fact]
    public async Task Validate_WhenWorkspaceIdEmpty_ShouldHaveValidationFailure()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(2);
        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Empty,
                Guid.Parse("8e2b4136-829c-11eb-8dcd-0242ac130003"),
                start,
                end
            )
        );

        var validator = new CreateReservationRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None
        );

        result.ShouldHaveValidationErrorFor(r => r.Dto.WorkspaceId);
    }

    [Fact]
    public async Task Validate_WhenUserIdEmpty_ShouldHaveValidationFailure()
    {
        var start = DateTimeOffset.UtcNow.AddHours(1);
        var end = start.AddHours(2);
        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Parse("8e2b4136-829c-11eb-8dcd-0242ac130003"),
                Guid.Empty,
                start,
                end
            )
        );

        var validator = new CreateReservationRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None
        );

        result.ShouldHaveValidationErrorFor(r => r.Dto.UserId);
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    public async Task Validate_WhenStartGreaterOrEqualThanEnd_ShouldHaveValidationFailure(
        int startHours,
        int endHours)
    {
        var baseDate = DateTimeOffset.UtcNow;

        var start = baseDate.AddHours(startHours);
        var end = baseDate.AddHours(endHours);

        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Parse("8e2b4136-829c-11eb-8dcd-0242ac130003"),
                Guid.Parse("8e2b4136-829c-11eb-8dcd-0242ac130005"),
                start,
                end
            )
        );

        var validator = new CreateReservationRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None
        );

        result.ShouldHaveValidationErrorFor(r => r.Dto.EndAt);
    }

    [Fact]
    public async Task Validate_WhenStartLessThanNow_ShouldHaveValidationFailure()
    {
        var now = DateTimeOffset.UtcNow;

        var start = now.AddHours(-1);
        var end = now.AddHours(2);

        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Parse("8e2b4136-829c-11eb-8dcd-0242ac130003"),
                Guid.Parse("8e2b4136-829c-11eb-8dcd-0242ac130005"),
                start,
                end
            )
        );

        var validator = new CreateReservationRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None
        );

        result.ShouldHaveValidationErrorFor(r => r.Dto.StartAt);
    }

    [Fact]
    public async Task Validate_WhenRequestIsValid_ShouldNotHaveValidationFailure()
    {
        var now = DateTimeOffset.UtcNow;

        var start = now.AddHours(1);
        var end = now.AddHours(2);

        var request = new CreateReservationRequest(
            new CreateReservationDto(
                Guid.Parse("8e2b4136-829c-11eb-8dcd-0242ac130003"),
                Guid.Parse("8e2b4136-829c-11eb-8dcd-0242ac130005"),
                start,
                end
            )
        );

        var validator = new CreateReservationRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None
        );

        result.ShouldNotHaveAnyValidationErrors();
    }
}
