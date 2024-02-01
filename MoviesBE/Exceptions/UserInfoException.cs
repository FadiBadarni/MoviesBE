using System.Net;

namespace MoviesBE.Exceptions;

public class UserInfoException : Exception
{
    public UserInfoException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}