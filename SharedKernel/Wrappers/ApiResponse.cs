namespace SharedKernel.Wrappers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public int Status { get; set; }

    public static ApiResponse<T> Ok(T data, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Status = statusCode,
            Message = null
        };
    }

    public static ApiResponse<T> Fail(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Status = statusCode,
            Message = message
        };
    }
}