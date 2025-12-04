// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Core.Exceptions;

public class CollectionPermissionAlreadyExistsException : Exception
{
    public CollectionPermissionAlreadyExistsException(Guid collectionId, string email)
        : base($"Collection {collectionId} has already a permission with email {email}", null)
    {
    }
}
