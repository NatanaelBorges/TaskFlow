namespace TaskFlow.Domain.SharedContext.UseCases;

public sealed class Result<T>
{
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string error, ResultErrorType errorType)
    {
        IsSuccess = false;
        Error = error;
        ErrorType = errorType;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType? ErrorType { get; }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> NotFound(string error) =>
        new(error, ResultErrorType.NotFound);

    public static Result<T> ValidationError(string error) =>
        new(error, ResultErrorType.Validation);

    public static Result<T> Unexpected(string error) =>
        new(error, ResultErrorType.Unexpected);
}

public enum ResultErrorType
{
    NotFound,
    Validation,
    Unexpected
}