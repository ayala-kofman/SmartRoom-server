using Microsoft.Extensions.Options;
using SmartRoom.Core.Interfaces.Services;
using SmartRoom.Core.Options;
using SmartRoom.Core.Results;

namespace SmartRoom.API.Middleware;

/// <summary>
/// The site is closed on Shabbat: between ShabbatCloseTime on Friday and ShabbatOpenTime on Saturday night
/// (Israel time) every request, login included, gets 503 in the uniform ErrorResponse shape.
/// Runs after the CorrelationId, so blocked requests are logged with an id too.
/// </summary>
public class ShabbatClosureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ShabbatClosureMiddleware> _logger;

    public ShabbatClosureMiddleware(RequestDelegate next, ILogger<ShabbatClosureMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // IBookingWindowService is Scoped, so it is taken per request here and not in the constructor
    // (a middleware is created once, and holding a Scoped service there would be a captive dependency).
    public async Task InvokeAsync(HttpContext context, IBookingWindowService window, IOptions<BookingRulesOptions> rules)
    {
        if (!window.IsSiteClosedNow())
        {
            await _next(context);
            return;
        }

        var openTime = rules.Value.ShabbatOpenTime.ToString("HH:mm");
        _logger.LogInformation("Request {Method} {Path} blocked: the site is closed for Shabbat until {OpenTime}.",
            context.Request.Method, context.Request.Path, openTime);

        await ErrorResponseWriter.WriteAsync(context, StatusCodes.Status503ServiceUnavailable, ErrorCodes.SiteClosed,
            $"The site is closed for Shabbat. It reopens on Saturday night at {openTime} (Israel time).");
    }
}
