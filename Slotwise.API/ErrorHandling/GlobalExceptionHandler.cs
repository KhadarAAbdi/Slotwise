using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Slotwise.Application.Sessions.Exceptions;
using Slotwise.Domain.Sessions.Entities;

namespace Slotwise.API.ErrorHandling
{
    /// <summary>
    /// Turns the exceptions the application raises on purpose into proper HTTP errors.
    /// Anything it doesn't recognise is left to the framework, which returns a 500.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title) = Classify(exception);

            if (status is null)
            {
                _logger.LogError(exception, "Unhandled exception");
                status = StatusCodes.Status500InternalServerError;
                title = "Something went wrong";
            }
            else
            {
                _logger.LogWarning("{Title} ({Status}): {Detail}", title, status, exception.Message);
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError ? "An unexpected error occurred" : exception.Message
            };

            httpContext.Response.StatusCode = status.Value;
            await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
            return true;
        }

        private static (int? Status, string Title) Classify(Exception exception)
        {
            switch (exception)
            {
                case SessionNotFoundException:
                    return (StatusCodes.Status404NotFound, "Session not found");

                case SessionConcurrencyException:
                    return (StatusCodes.Status409Conflict, "The session was changed by someone else");

                case ArgumentException or InvalidOperationException when RaisedByDomain(exception):
                    return (StatusCodes.Status400BadRequest, "The request breaks a booking rule");

                default:
                    return (null, string.Empty);
            }
        }

        private static bool RaisedByDomain(Exception exception)
        {
            return exception.TargetSite?.DeclaringType?.Assembly == typeof(Session).Assembly;
        }
    }
}
