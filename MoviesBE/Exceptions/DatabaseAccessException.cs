namespace MoviesBE.Exceptions;

[Serializable]
public class DatabaseAccessException : Exception
{
    public DatabaseAccessException()
        : base("An error occurred while accessing the database.")
    {
    }

    public DatabaseAccessException(string message)
        : base(message)
    {
    }

    public DatabaseAccessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}