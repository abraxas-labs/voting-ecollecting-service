// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;

public interface IPermissionService : Shared.Abstractions.Core.Services.IPermissionService
{
    DateTime Now { get; }

    bool IsAuthenticated { get; }

    string UserFirstName { get; }

    string UserLastName { get; }

    bool UserEmailVerified { get; }

    void SetCreated(IAuditedEntity entity);

    void SetCreatedWithoutPII(IAuditedEntity entity);

    void SetModified(IAuditedEntity? entity);

    Task<string?> GetSocialSecurityNumber();

    void Init(
        string userId,
        string userName,
        string userEmail,
        bool userEmailVerified,
        string userFirstName,
        string userLastName);

    void RequireEmail(string email);
}
