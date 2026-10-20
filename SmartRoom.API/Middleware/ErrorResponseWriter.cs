using SmartRoom.API.Models;

namespace SmartRoom.API.Middleware;

/// <summary>
/// Writes the uniform ErrorResponse from code that runs outside the controllers
/// (middleware, JWT challenge/forbid), so every error has the same JSON shape.
/// </summary>
public static class ErrorResponseWriter
{
    public static Task WriteAsync(HttpContext context, int status, string code, string message)
    {
        context.Response.StatusCode = status;
        var body = new ErrorResponse(status, code, message, context.TraceIdentifier);
        return context.Response.WriteAsJsonAsync(body);
    }
}
