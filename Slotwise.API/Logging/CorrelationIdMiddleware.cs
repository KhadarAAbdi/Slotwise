using Serilog.Context;

namespace Slotwise.API.Logging
{
    /// <summary>
    /// Gives every request an id and attaches it to every log line written while the request runs, so one
    /// booking can be followed through the controller, handlers, repository and projection.
    /// A caller can supply its own id in the X-Correlation-Id header (useful when another service calls this one),
    /// and the same id is echoed back on the response so the caller can quote it when reporting a problem.
    /// </summary>
    public sealed class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers[HeaderName].FirstOrDefault();

            if (!IsAcceptable(correlationId))
            {
                correlationId = Guid.NewGuid().ToString("N");
            }

            context.Response.Headers[HeaderName] = correlationId;

            var token = LogContext.PushProperty("CorrelationId", correlationId);

            try
            {
                await _next(context);
            }
            finally
            {
                token.Dispose();
            }
        }
        private static bool IsAcceptable(string? value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && value.Length <= 64
                && value.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');
        }
    }
}
