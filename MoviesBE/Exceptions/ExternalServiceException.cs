namespace MoviesBE.Exceptions;

public class ExternalServiceException : Exception
{
    public ExternalServiceException()
        : base("An error occurred while calling an external service.")
    {
    }

    public ExternalServiceException(string message)
        : base(message)
    {
    }

    public ExternalServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}