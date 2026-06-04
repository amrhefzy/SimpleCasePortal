namespace SimpleCasePortal.Application.Common;

public sealed class ApiResponse<T>
{
    private ApiResponse(bool success, string message, T? data, IReadOnlyCollection<string> errors)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors;
    }

    public bool Success { get; }

    public string Message { get; }

    public T? Data { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static ApiResponse<T> Ok(T data, string message = "Request completed successfully.")
    {
        return new ApiResponse<T>(true, message, data, Array.Empty<string>());
    }

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null)
    {
        return new ApiResponse<T>(false, message, default, errors?.ToArray() ?? Array.Empty<string>());
    }
}
