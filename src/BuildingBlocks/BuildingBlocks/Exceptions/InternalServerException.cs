namespace BuildingBlocks.Exceptions;

public class InternalServerException : BaseException
{
    public InternalServerException(string message)
        : base(message) { }

    public InternalServerException(string message, Exception innerException)
        : base(message, innerException) { }
}

