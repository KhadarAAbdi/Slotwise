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
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (status, title) = Classify(exception);

            if (status is null)
            {
                return false;
            }

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message
            };

            httpContext.Response.StatusCode = status.Value;
            await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);

            return true;
        }

        private static (int? Status, string Title) Classify(Exception exception)
        {
            return exception switch
            {
                SessionNotFoundException => (StatusCodes.Status404NotFound, "Session not found"),
                SessionConcurrencyException => (StatusCodes.Status409Conflict, "The session was changed by someone else"),
                ArgumentException or InvalidOperationException when RaisedByDomain(exception) =>
                    (StatusCodes.Status400BadRequest, "The request breaks a booking rule"),
                _ => (null, string.Empty)
            };
        }

        // The domain signals rule violations with ArgumentException and InvalidOperationException. The same
        // types also come out of the framework when something is genuinely broken, so only those thrown
        // from the Domain assembly are treated as the caller's mistake.
        private static bool RaisedByDomain(Exception exception)
        {
            return exception.TargetSite?.DeclaringType?.Assembly == typeof(Session).Assembly;
        }
    }
}
