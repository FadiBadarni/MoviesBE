namespace MoviesBE.Services;

public class ServiceResult<T>
{
    public ServiceResult(T data, bool success, string errorMessage)
    {
        Data = data;
        Success = success;
        ErrorMessage = errorMessage;
    }

    public T Data { get; }
    public bool Success { get; }
    public string ErrorMessage { get; }
}