// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(object id)
        : base($"Entity with id {id} not found")
    {
    }

    public EntityNotFoundException(string type, object id)
       : base($"{type} with id {id} not found")
    {
    }

    public EntityNotFoundException(Type type, object id)
       : base($"{type} with id {id} not found")
    {
    }

    public EntityNotFoundException(string? message, Exception? innerException)
    : base(message, innerException)
    {
    }
}
