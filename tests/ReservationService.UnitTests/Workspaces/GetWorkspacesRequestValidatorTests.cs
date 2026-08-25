using FluentValidation.TestHelper;
using ReservationService.Endpoints.Workspace.Get;

namespace ReservationService.UnitTests.Workspaces;

public class GetWorkspacesRequestValidatorTests
{
    [Fact]
    public async Task Validate_WhenPageNumberLessOne_ShouldHaveError()
    {
        var request = new GetWorkspacesRequest(
            Name: "",
            IsActive: true,
            PageNumber: 0,
            PageSize: 5
        );

        var validator = new GetWorkspacesRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        result.ShouldHaveValidationErrorFor(r => r.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task Validate_WhenPageSizeTooBig_ShouldHaveError(int pageSize)
    {
        var request = new GetWorkspacesRequest(
            Name: "",
            IsActive: true,
            PageNumber: 1,
            PageSize: pageSize
        );

        var validator = new GetWorkspacesRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None); ;

        result.ShouldHaveValidationErrorFor(r => r.PageSize);
    }

    [Fact]
    public async Task Validate_WhenNameTooLong_ShouldHaveError()
    {
        var request = new GetWorkspacesRequest(
            Name: "VipLuxury TopOne TheBestALlTime",
            IsActive: true,
            PageNumber: 1,
            PageSize: 5
        );

        var validator = new GetWorkspacesRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public async Task Validate_WhenRequestValid_ShouldNotHaveError()
    {
        var request = new GetWorkspacesRequest(
            Name: "Место номер 5",
            IsActive: true,
            PageNumber: 1,
            PageSize: 5
        );

        var validator = new GetWorkspacesRequestValidator();

        var result = await validator.TestValidateAsync(
            objectToTest: request,
            cancellationToken: CancellationToken.None);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
