using SmartRoom.Core.Results;

namespace SmartRoom.API.Middleware;

/// <summary>
/// First in the pipeline: catches any exception nothing else handled, logs it at Error
/// and returns 500 in the uniform shape — never a stack trace to the client.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            using var scope = BeginCorrelationScope(context);
            // The client closed the request; not a server error, and nobody is waiting for an answer.
            _logger.LogInformation("Request {Method} {Path} was cancelled by the client.",
                context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            using var scope = BeginCorrelationScope(context);
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}.", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                // Headers already sent; the response can't be replaced, so let the server abort it.
                throw;
            }

            context.Response.Clear();
            await ErrorResponseWriter.WriteAsync(context, StatusCodes.Status500InternalServerError,
                ErrorCodes.InternalError, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// This middleware sits outside CorrelationIdMiddleware, whose log scope has ended by the time an exception gets here;
    /// the id it put in TraceIdentifier is still there, so the Error line keeps the same CorrelationId.
    /// </summary>
    private IDisposable? BeginCorrelationScope(HttpContext context) =>
        _logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = context.TraceIdentifier });
}
