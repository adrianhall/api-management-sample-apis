using System.Runtime.Serialization;

namespace Todo.GraphQLApi.GraphQL;

/// <summary>
/// An exception that is thrown when trying to save an item
/// to a non-existent list.
/// </summary>
public class ListDoesNotExistException : Exception
{
    public ListDoesNotExistException()
    {
    }

    public ListDoesNotExistException(string? message) : base(message)
    {
    }

    public ListDoesNotExistException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ListDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
