using Microsoft.AspNetCore.Diagnostics;

namespace ReservationService.Handlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(g => g.ErrorMessage)
                    .ToArray());

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        var problemDetails = new ValidationProblemDetails()
        {
            Title = exception.GetType().Name,
            Status = httpContext.Response.StatusCode,
            Instance = httpContext.Request.Path,
            Errors = errors,
            Detail = "One or more validation errors occurred."
        };
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
