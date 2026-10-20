using System.Text.Json.Serialization;

namespace SmartRoom.API.Models;

/// <summary>
/// The one JSON shape of every error the API returns (service results, validation, 401/403, 500, Shabbat 503).
/// </summary>
/// <param name="Status">The HTTP status code, repeated in the body for the client's convenience.</param>
/// <param name="Code">Short stable code (see Core ErrorCodes), e.g. SLOT_TAKEN.</param>
/// <param name="Message">Message to show the user.</param>
/// <param name="CorrelationId">Same id as in the response header and the server logs.</param>
/// <param name="Errors">Invalid fields and their messages; only on a 400 from model validation.</param>
public record ErrorResponse(
    int Status,
    string Code,
    string Message,
    string CorrelationId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IDictionary<string, string[]>? Errors = null);
