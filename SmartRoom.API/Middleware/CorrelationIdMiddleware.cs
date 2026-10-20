using System.Diagnostics;

namespace SmartRoom.API.Middleware;

/// <summary>
/// Gives every request one id: taken from the X-Correlation-ID header or newly created,
/// returned in the response header, put in the log scope (so every log line of the request carries it)
/// and in TraceIdentifier (so every ErrorResponse carries it). Also writes one log line per request.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private const int MaxIdLength = 64;

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ReadOrCreate(context);
        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            var stopwatch = Stopwatch.StartNew();
            var failed = false;
            try
            {
                await _next(context);
            }
            catch
            {
                // The exception middleware (outside this one) turns it into a 500.
                failed = true;
                throw;
            }
            finally
            {
                var status = failed ? StatusCodes.Status500InternalServerError : context.Response.StatusCode;
                // Path only — no query string, headers or body, so no ID number or token can reach the log.
                _logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms",
                    context.Request.Method, context.Request.Path, status, stopwatch.ElapsedMilliseconds);
            }
        }
    }

    /// <summary>A client-sent id is kept only if it is short and safe to echo; otherwise a new one is made.</summary>
    private static string ReadOrCreate(HttpContext context)
    {
        var sent = context.Request.Headers[HeaderName].ToString();
        var isSafe = sent.Length is > 0 and <= MaxIdLength
            && sent.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.');
        return isSafe ? sent : Guid.NewGuid().ToString();
    }
}
