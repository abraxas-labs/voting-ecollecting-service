// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;

/// <summary>
/// Permission service to check users permissions and roles.
/// </summary>
public interface IPermissionService : Shared.Abstractions.Core.Services.IPermissionService
{
    /// <summary>
    /// Gets the tenant id of the current user.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Gets the current timestamp in UTC.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the BFS access control lists for the current user.
    /// </summary>
    AclBfsLists AclBfsLists { get; }

    /// <summary>
    /// Gets the name of the current user.
    /// </summary>
    string UserName { get; }

    bool HasRole(string role);

    void SetAccessControlPermissions(AclBfsLists aclBfsLists);

    /// <summary>
    /// Sets the configured Abraxas service user authentication if no authentication is currently provided.
    /// This should only be used for background jobs or similar things.
    /// </summary>
    void SetAbraxasAuthIfNotAuthenticated();

    /// <summary>
    /// Marks an entity as created as of now by the current user.
    /// </summary>
    /// <param name="entity">The entity.</param>
    void SetCreated(IAuditedEntity entity);

    /// <summary>
    /// Marks an entity as modified as of now by the current user.
    /// </summary>
    /// <param name="entity">The entity.</param>
    void SetModified(IAuditedEntity entity);
}
