namespace SmartRoom.Core.Results;

/// <summary>
/// Outcome of a service operation, in business terms (not HTTP).
/// The API layer is the only place that maps these to status codes.
/// </summary>
public enum ResultStatus
{
    Success = 1,
    Created = 2,
    NotFound = 3,
    ValidationError = 4,
    Unauthorized = 5,
    Forbidden = 6,
    Conflict = 7
}
