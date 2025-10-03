
namespace RestaurantApp.Shared.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public int StatusCode { get; }
    
    protected Result(bool isSuccess, string? error, int statusCode = 200)
    {
        if (isSuccess && !string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Success result cannot have an error message");
        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Failed result must have an error message");
            
        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }
    
    public static Result Success() => new(true, null, 200);
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
    
    // Helper methods dla częstych przypadków
    public static Result NotFound(string error) => new(false, error, 404);
    public static Result Unauthorized(string error) => new(false, error, 401);
    public static Result Forbidden(string error) => new(false, error, 403);
    public static Result Conflict(string error) => new(false, error, 409);
    public static Result ValidationError(string error) => new(false, error, 400);
    public static Result InternalError(string error) => new(false, error, 500);
    
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error, int statusCode = 400) => Result<T>.Failure(error, statusCode);
}

public class Result<T> : Result
{
    public T? Value { get; }
    
    protected internal Result(T? value, bool isSuccess, string? error, int statusCode = 200) 
        : base(isSuccess, error, statusCode)
    {
        Value = value;
    }
    
    public static Result<T> Success(T value, int statusCode = 200) => new(value, true, null, statusCode);
    public static Result<T> Created(T value) => new(value, true, null, 201);
    public new static Result<T> Failure(string error, int statusCode = 400) => new(default, false, error, statusCode);
    
    // Helper methods dla częstych przypadków
    public new static Result<T> NotFound(string error) => new(default, false, error, 404);
    public new static Result<T> Unauthorized(string error) => new(default, false, error, 401);
    public new static Result<T> Forbidden(string error) => new(default, false, error, 403);
    public new static Result<T> Conflict(string error) => new(default, false, error, 409);
    public new static Result<T> ValidationError(string error) => new(default, false, error, 400);
    public new static Result<T> InternalError(string error) => new(default, false, error, 500);
}

// Extension methods dla konwersji Result na IActionResult
