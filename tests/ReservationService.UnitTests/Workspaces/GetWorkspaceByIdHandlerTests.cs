using FluentValidation;
using FluentValidation.Results;
using Moq;
using ReservationService.Endpoints.Workspace.GetById;
using ReservationService.Exceptions;
using ReservationService.Infrastructure.Repositories;
using ReservationService.Models;

namespace ReservationService.UnitTests.Workspaces;

public class GetWorkspaceByIdHandlerTests
{
    [Fact]
    public async Task GetWorkspaceById_WhenRequestInvalid_ShouldThrowValidationException()
    {
        var request = new GetWorkspaceByIdRequest(
            Guid.Empty
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var validator = new Mock<IValidator<GetWorkspaceByIdRequest>>();

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("", " ")]));

        var handler = new GetWorkspaceByIdHandler(
            workspaceRepository.Object,
            validator.Object);

        await Assert
            .ThrowsAsync<ValidationException>(() => handler.GetWorkspaceByIdAsync(
                    request,
                    CancellationToken.None));

        workspaceRepository
            .Verify(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetWorkspaceById_WhenWorkspaceNotContains_ShouldThrowNotFoundException()
    {
        var request = new GetWorkspaceByIdRequest(
            Guid.Empty
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var validator = new Mock<IValidator<GetWorkspaceByIdRequest>>();

        workspaceRepository
            .Setup(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Workspace?)null);

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new GetWorkspaceByIdHandler(
            workspaceRepository.Object,
            validator.Object);

        await Assert
            .ThrowsAsync<NotFoundException>(() => handler.GetWorkspaceByIdAsync(
                    request,
                    CancellationToken.None));

        workspaceRepository
            .Verify(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetWorkspaceById_WhenRequestValidAndWorkspaceContains_ShouldReturnWorkspace()
    {
        var workspace = new Workspace()
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Workspace 1",
            Description = "Test workspace 1",
            PricePerHour = 100m,
            IsActive = true
        };

        var request = new GetWorkspaceByIdRequest(
            Guid.Parse("00000000-0000-0000-0000-000000000001")
        );

        var workspaceRepository = new Mock<IWorkspaceRepository>();
        var validator = new Mock<IValidator<GetWorkspaceByIdRequest>>();

        workspaceRepository
            .Setup(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(workspace);

        validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new GetWorkspaceByIdHandler(
            workspaceRepository.Object,
            validator.Object);

        var result = await handler.GetWorkspaceByIdAsync(
            request,
            CancellationToken.None);

        Assert.Equal(result.Id, workspace.Id);
        Assert.Equal(result.Name, workspace.Name);
        Assert.Equal(result.Description, workspace.Description);
        Assert.Equal(result.IsActive, workspace.IsActive);
        Assert.Equal(result.PricePerHour, workspace.PricePerHour);

        workspaceRepository
            .Verify(r => r.GetByIdAsync(
                request.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
