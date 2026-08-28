using Microsoft.AspNetCore.Diagnostics;
using ReservationService.Exceptions;

namespace ReservationService.Handlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception, CancellationToken cancellationToken)
    {
        (int statusCode, string title, string detail) = exception switch
        {
            NotFoundException => (
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound,
                exception.GetType().Name,
                exception.Message
            ),

            SlotNotAvailableException => (
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict,
                exception.GetType().Name,
                exception.Message
            ),

            WorkspaceNotActiveException => (
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict,
                exception.GetType().Name,
                exception.Message
            ),

            ConfirmationForbiddenException => (
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict,
                exception.GetType().Name,
                exception.Message
            ),

            _ => (
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError,
                "Необработанное исключение",
                "Ошибка сервера"
            )
        };

        if (statusCode == 500)
            logger.LogError(exception, "Unhandled exception");

        var problemDetails = new ProblemDetails()
        {
            Title = title,
            Status = statusCode,
            Instance = httpContext.Request.Path,
            Detail = detail
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
