namespace SmartRoom.Core.Exceptions;

/// <summary>
/// Thrown by a repository when a save lost a concurrency race
/// (EF's DbUpdateConcurrencyException, or a violation of the active-booking unique index).
/// Keeps EF types out of the Service layer; the service turns it into a Conflict result.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
