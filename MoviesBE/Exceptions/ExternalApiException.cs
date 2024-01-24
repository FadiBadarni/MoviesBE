namespace MoviesBE.Exceptions;

public class ExternalApiException : Exception
{
    public ExternalApiException()
        : base("An error occurred while calling an external API.")
    {
    }

    public ExternalApiException(string message)
        : base(message)
    {
    }

    public ExternalApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}