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

    public bool IsSuccess => Status is ResultStatus.Success or ResultStatus.Created;

    protected Result(ResultStatus status, string? error)
    {
        Status = status;
        Error = error;
    }

    public static Result Success() => new(ResultStatus.Success, null);

    public static Result NotFound(string error) => new(ResultStatus.NotFound, error);
    public static Result ValidationError(string error) => new(ResultStatus.ValidationError, error);
    public static Result Unauthorized(string error) => new(ResultStatus.Unauthorized, error);
    public static Result Forbidden(string error) => new(ResultStatus.Forbidden, error);
    public static Result Conflict(string error) => new(ResultStatus.Conflict, error);
}

/// <summary>Result that carries a value on success (e.g. the created booking DTO).</summary>
public class Result<T> : Result
{
    /// <summary>The value on success; default on failure.</summary>
    public T? Value { get; }

    private Result(ResultStatus status, T? value, string? error) : base(status, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(ResultStatus.Success, value, null);

    /// <summary>A new resource was created (API returns 201 with Location).</summary>
    public static Result<T> Created(T value) => new(ResultStatus.Created, value, null);

    public new static Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, error);
    public new static Result<T> ValidationError(string error) => new(ResultStatus.ValidationError, default, error);
    public new static Result<T> Unauthorized(string error) => new(ResultStatus.Unauthorized, default, error);
    public new static Result<T> Forbidden(string error) => new(ResultStatus.Forbidden, default, error);
    public new static Result<T> Conflict(string error) => new(ResultStatus.Conflict, default, error);
}
