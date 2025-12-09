
namespace RestaurantApp.Shared.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public List<string> Errors { get; } = new();
    public int StatusCode { get; }
    
    public string? Error => Errors.Any() ? string.Join("; ", Errors) : null;

    protected Result(bool isSuccess, IEnumerable<string>? errors, int statusCode = 200)
    {
        if (isSuccess && errors is not null && errors.Any())
            throw new InvalidOperationException("Success result cannot have error messages");

        if (!isSuccess && (errors is null || !errors.Any()))
            throw new InvalidOperationException("Failed result must have at least one error message");

        IsSuccess = isSuccess;

        if (errors != null)
            Errors = errors.ToList();

        StatusCode = statusCode;
    }
    
    public static Result Failure(string error, int statusCode = 400) =>
        new(false, new[] { error }, statusCode);
    
    public static Result Failure(IEnumerable<string> errors, int statusCode = 400) =>
        new(false, errors, statusCode);

    public static Result Success() => new(true, null, 200);
    
    public static Result NotFound(string error) => Failure(error, 404);
    public static Result Unauthorized(string error) => Failure(error, 401);
    public static Result Forbidden(string error) => Failure(error, 403);
    public static Result Conflict(string error) => Failure(error, 409);
    public static Result ValidationError(string error) => Failure(error, 400);
    
    public static Result ValidationError(IEnumerable<string> errors) => Failure(errors, 400);
    public static Result InternalError(string error) => Failure(error, 500);

    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error, int statusCode = 400) => Result<T>.Failure(error, statusCode);
}

public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(
        T? value,
        bool isSuccess,
        IEnumerable<string>? errors,
        int statusCode = 200) 
        : base(isSuccess, errors, statusCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value, int statusCode = 200) =>
        new(value, true, null, statusCode);

    public static Result<T> Created(T value) =>
        new(value, true, null, 201);
    
    public new static Result<T> Failure(string error, int statusCode = 400) =>
        new(default, false, new[] { error }, statusCode);
    
    public static Result<T> Failure(IEnumerable<string> errors, int statusCode = 400) =>
        new(default, false, errors, statusCode);
    
    public new static Result<T> NotFound(string error) => Failure(error, 404);
    public new static Result<T> Unauthorized(string error) => Failure(error, 401);
    public new static Result<T> Forbidden(string error) => Failure(error, 403);
    public new static Result<T> Conflict(string error) => Failure(error, 409);
    public new static Result<T> ValidationError(string error) => Failure(error, 400);
    public new static Result<T> ValidationError(IEnumerable<string> errors) => Failure(errors, 400);
    public new static Result<T> InternalError(string error) => Failure(error, 500);
    
    public static Result<T> From(Result result)
    {
        return new Result<T>(
            default,
            result.IsSuccess,
            result.IsFailure ? result.Errors : null,
            result.StatusCode
        );
    }

}

