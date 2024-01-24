namespace MoviesBE.Exceptions;

public class DataFormatException : Exception
{
    public DataFormatException()
        : base("An error occurred while formatting data.")
    {
    }

    public DataFormatException(string message)
        : base(message)
    {
    }

    public DataFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}