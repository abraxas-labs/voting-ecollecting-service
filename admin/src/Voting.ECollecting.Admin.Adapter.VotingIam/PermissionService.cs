// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Adapter.VotingIam.Configuration;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Common;
using Voting.Lib.Iam.Models;
using Voting.Lib.Iam.Store;

namespace Voting.ECollecting.Admin.Adapter.VotingIam;

/// <inheritdoc/>
public class PermissionService : IPermissionService
{
    private readonly IAuth _auth;
    private readonly IAuthStore _authStore;
    private readonly ILogger<PermissionService> _logger;
    private readonly VotingIamConfig _config;
    private readonly TimeProvider _timeProvider;

    public PermissionService(
        IAuth auth,
        IAuthStore authStore,
        ILogger<PermissionService> logger,
        VotingIamConfig config,
        TimeProvider timeProvider)
    {
        _auth = auth;
        _authStore = authStore;
        _logger = logger;
        _config = config;
        _timeProvider = timeProvider;
    }

    public string UserId => _auth.User.Loginid;

    public string? UserEmail => _auth.User.PrimaryOrFirstEmail;

    public string TenantId => _auth.Tenant.Id;

    public DateOnly Today => _timeProvider.GetUtcTodayDateOnly();

    public AclBfsLists AclBfsLists { get; private set; } = AclBfsLists.Empty;

    public string UserName => string.IsNullOrWhiteSpace(_auth.User.Username)
        ? _auth.User.Servicename ?? "unknown"
        : $"{_auth.User.Firstname} {_auth.User.Lastname}";

    public void SetAccessControlPermissions(AclBfsLists aclBfsLists)
    {
        AclBfsLists = aclBfsLists;
    }

    public void SetAbraxasAuthIfNotAuthenticated()
    {
        if (!_auth.IsAuthenticated)
        {
            _logger.LogDebug(SecurityLogging.SecurityEventId, "Using Abraxas authentication values, since no user is authenticated");
            _authStore.SetValues(
                string.Empty,
                new User
                {
                    Loginid = _config.ServiceUserId,
                    Servicename = _config.ServiceAccount,
                },
                new Tenant
                {
                    Id = _config.AbraxasTenantId,
                },
                []);
        }
    }

    // if adjusted, check for setProperty calls in db queries.
    public virtual void SetCreated(IAuditedEntity entity)
    {
        entity.AuditInfo.CreatedAt = _timeProvider.GetUtcNowDateTime();
        entity.AuditInfo.CreatedById = UserId;
        entity.AuditInfo.CreatedByName = UserName;
        entity.AuditInfo.CreatedByEmail = UserEmail;
    }

    // if adjusted, check for setProperty calls in db queries.
    public virtual void SetModified(IAuditedEntity entity)
    {
        entity.AuditInfo.ModifiedAt = _timeProvider.GetUtcNowDateTime();
        entity.AuditInfo.ModifiedById = UserId;
        entity.AuditInfo.ModifiedByName = UserName;
        entity.AuditInfo.ModifiedByEmail = UserEmail;
    }

    public bool HasRole(string role)
    {
        return _auth.HasRole(role);
    }
}
