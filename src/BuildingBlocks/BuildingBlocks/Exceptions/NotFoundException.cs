namespace BuildingBlocks.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message)
        : base(message) { }

    public NotFoundException(string name, object key)
        : base($"\"{name}\" with id ({key}) was not found.") { }
}

