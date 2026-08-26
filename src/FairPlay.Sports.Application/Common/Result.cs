namespace FairPlay.Sports.Application.Common;

public enum ResultErrorType
{
    None,
    Validation,
    NotFound
}

public sealed class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ResultErrorType.None);

    public static Result Failure(string error, ResultErrorType errorType = ResultErrorType.Validation) =>
        new(false, error, errorType);

    public static Result NotFound(string error) => new(false, error, ResultErrorType.NotFound);
}

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.None);

    public static Result<T> Failure(string error, ResultErrorType errorType = ResultErrorType.Validation) =>
        new(false, default, error, errorType);

    public static Result<T> NotFound(string error) => new(false, default, error, ResultErrorType.NotFound);
}
