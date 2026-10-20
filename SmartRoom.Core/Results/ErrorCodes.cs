namespace SmartRoom.Core.Results;

/// <summary>
/// Short, stable codes returned to the client with every error, so the client can react
/// to a specific case (e.g. SLOT_TAKEN → pick another room) without parsing the message.
/// </summary>
public static class ErrorCodes
{
    // General codes, one per failure kind; used when no specific code is given.
    public const string NotFound = "NOT_FOUND";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string Conflict = "CONFLICT";
    public const string InternalError = "INTERNAL_ERROR";
    public const string SiteClosed = "SITE_CLOSED";

    // Bookings
    public const string SlotTaken = "SLOT_TAKEN";
    public const string RoomNotActive = "ROOM_NOT_ACTIVE";
    public const string TeacherBusy = "TEACHER_BUSY";
    public const string ClassBusy = "CLASS_BUSY";
    public const string AlreadyCancelled = "ALREADY_CANCELLED";
    public const string ConcurrentChange = "CONCURRENT_CHANGE";

    // Rooms
    public const string RoomNumberTaken = "ROOM_NUMBER_TAKEN";
    public const string RoomNotClosed = "ROOM_NOT_CLOSED";
    public const string RoomHasBookings = "ROOM_HAS_BOOKINGS";

    // Users
    public const string IdentityNumberTaken = "IDENTITY_NUMBER_TAKEN";
}
