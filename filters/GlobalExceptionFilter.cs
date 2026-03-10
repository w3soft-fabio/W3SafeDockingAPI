using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebSafeDockingAPI.Exceptions;

namespace WebSafeDockingAPI.Filters;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        int statusCode = exception switch
        {
            NotFoundException      => StatusCodes.Status404NotFound,
            ValidationException    => StatusCodes.Status400BadRequest,
            ArgumentException      => StatusCodes.Status400BadRequest,
            _                      => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            logger.LogWarning("Request error ({StatusCode}): {Message}", statusCode, exception.Message);

        context.Result = new ObjectResult(new { error = exception.Message })
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
