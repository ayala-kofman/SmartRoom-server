namespace SmartRoom.Core.Results;

/// <summary>
/// Returned by every service instead of throwing for expected failures
/// (not found, validation, permission, conflict).
/// </summary>
public class Result
{
    public ResultStatus Status { get; }

    /// <summary>Message for the client when the operation failed; null on success.</summary>
    public string? Error { get; }

    /// <summary>Short code for the client (see <see cref="ErrorCodes"/>); null on success.</summary>
    public string? ErrorCode { get; }

    public bool IsSuccess => Status is ResultStatus.Success or ResultStatus.Created;

    protected Result(ResultStatus status, string? error, string? errorCode)
    {
        Status = status;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(ResultStatus.Success, null, null);

    public static Result NotFound(string error) => new(ResultStatus.NotFound, error, ErrorCodes.NotFound);
    public static Result ValidationError(string error) => new(ResultStatus.ValidationError, error, ErrorCodes.ValidationError);
    public static Result Unauthorized(string error) => new(ResultStatus.Unauthorized, error, ErrorCodes.Unauthorized);
    public static Result Forbidden(string error) => new(ResultStatus.Forbidden, error, ErrorCodes.Forbidden);

    /// <param name="errorCode">A specific code from <see cref="ErrorCodes"/>, so the client can tell conflicts apart.</param>
    public static Result Conflict(string error, string errorCode = ErrorCodes.Conflict) =>
        new(ResultStatus.Conflict, error, errorCode);
}

/// <summary>Result that carries a value on success (e.g. the created booking DTO).</summary>
public class Result<T> : Result
{
    /// <summary>The value on success; default on failure.</summary>
    public T? Value { get; }

    private Result(ResultStatus status, T? value, string? error, string? errorCode) : base(status, error, errorCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(ResultStatus.Success, value, null, null);

    /// <summary>A new resource was created (API returns 201 with Location).</summary>
    public static Result<T> Created(T value) => new(ResultStatus.Created, value, null, null);

    public new static Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, error, ErrorCodes.NotFound);
    public new static Result<T> ValidationError(string error) => new(ResultStatus.ValidationError, default, error, ErrorCodes.ValidationError);
    public new static Result<T> Unauthorized(string error) => new(ResultStatus.Unauthorized, default, error, ErrorCodes.Unauthorized);
    public new static Result<T> Forbidden(string error) => new(ResultStatus.Forbidden, default, error, ErrorCodes.Forbidden);

    /// <param name="errorCode">A specific code from <see cref="ErrorCodes"/>, so the client can tell conflicts apart.</param>
    public new static Result<T> Conflict(string error, string errorCode = ErrorCodes.Conflict) =>
        new(ResultStatus.Conflict, default, error, errorCode);
}
