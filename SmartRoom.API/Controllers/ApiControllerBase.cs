using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartRoom.API.Models;
using SmartRoom.Core.Enums;
using SmartRoom.Core.Models;
using SmartRoom.Core.Results;

namespace SmartRoom.API.Controllers;

/// <summary>
/// Base of every controller: the one place that turns a service Result into an HTTP response,
/// and that reads the logged-in user from the token. Controllers stay a few lines each.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// The logged-in user, from the token claims only — a teacher id never comes from the request body.
    /// Only for actions that require login (the fallback policy guarantees a validated token).
    /// </summary>
    protected CurrentUser CurrentUser
    {
        get
        {
            var id = int.Parse(RequiredClaim(ClaimTypes.NameIdentifier), CultureInfo.InvariantCulture);
            var role = Enum.Parse<UserRole>(RequiredClaim(ClaimTypes.Role));
            var classGroupClaim = User.FindFirstValue(AppClaimTypes.ClassGroupId);
            int? classGroupId = classGroupClaim is null
                ? null
                : int.Parse(classGroupClaim, CultureInfo.InvariantCulture);

            return new CurrentUser(id, role, classGroupId);
        }
    }

    /// <summary>Success → 204 No Content; failure → the uniform error.</summary>
    protected IActionResult FromResult(Result result) =>
        result.IsSuccess ? NoContent() : Error(result);

    /// <summary>
    /// Success → 200 with the value; Created → 201 with the value but no Location
    /// (for resources without a GET-by-id endpoint, e.g. users); failure → the uniform error.
    /// </summary>
    protected IActionResult FromResult<T>(Result<T> result) => result.Status switch
    {
        ResultStatus.Success => Ok(result.Value),
        ResultStatus.Created => StatusCode(StatusCodes.Status201Created, result.Value),
        _ => Error(result)
    };

    /// <summary>Created → 201 with a Location header that points to <paramref name="actionName"/>; failure → the uniform error.</summary>
    /// <param name="routeValues">Builds the route of the new resource from the created value, e.g. dto => new { id = dto.Id }.</param>
    protected IActionResult FromCreated<T>(Result<T> result, string actionName, Func<T, object> routeValues) =>
        result.IsSuccess ? CreatedAtAction(actionName, routeValues(result.Value!), result.Value) : Error(result);

    private ObjectResult Error(Result result)
    {
        var status = result.Status switch
        {
            ResultStatus.NotFound => StatusCodes.Status404NotFound,
            ResultStatus.ValidationError => StatusCodes.Status400BadRequest,
            ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
            ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
            ResultStatus.Conflict => StatusCodes.Status409Conflict,
            _ => throw new InvalidOperationException($"Result status {result.Status} is not a failure.")
        };

        // TraceIdentifier is set to the CorrelationId by its middleware (stage 7).
        var body = new ErrorResponse(status, result.ErrorCode!, result.Error!, HttpContext.TraceIdentifier);
        return StatusCode(status, body);
    }

    private string RequiredClaim(string type) =>
        User.FindFirstValue(type)
        ?? throw new InvalidOperationException($"The token has no '{type}' claim.");
}
