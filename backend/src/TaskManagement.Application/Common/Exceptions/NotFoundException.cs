namespace TaskManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist. Mapped to HTTP 404 by the API layer.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"{name} with key '{key}' was not found.")
    {
    }
}
