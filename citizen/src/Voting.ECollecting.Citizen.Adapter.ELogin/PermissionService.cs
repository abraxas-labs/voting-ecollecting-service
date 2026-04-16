// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Citizen.Adapter.ELogin;

public class PermissionService : IPermissionService
{
    private const string NoPIIName = "Citizen";

    private readonly TimeProvider _timeProvider;
    private readonly SocialSecurityNumberCache _ssnCache;
    private readonly PersonServiceClient _personServiceClient;

    public PermissionService(TimeProvider timeProvider, SocialSecurityNumberCache ssnCache, PersonServiceClient personServiceClient)
    {
        _timeProvider = timeProvider;
        _ssnCache = ssnCache;
        _personServiceClient = personServiceClient;
    }

    public string UserId { get; private set; } = string.Empty;

    public DateTime Now => _timeProvider.GetUtcNowDateTime();

    public bool IsAuthenticated { get; private set; }

    public string UserName { get; private set; } = string.Empty;

    public string UserEmail { get; private set; } = string.Empty;

    public bool UserEmailVerified { get; private set; }

    public DateTimeOffset AuthenticatedTime { get; private set; }

    public string UserFirstName { get; private set; } = string.Empty;

    public string UserLastName { get; private set; } = string.Empty;

    public virtual void SetCreated(IAuditedEntity entity)
    {
        entity.AuditInfo.CreatedAt = _timeProvider.GetUtcNowDateTime();
        entity.AuditInfo.CreatedById = UserId;
        entity.AuditInfo.CreatedByName = UserName;
        entity.AuditInfo.CreatedByEmail = UserEmail;
    }

    public void SetCreatedWithoutPII(IAuditedEntity entity)
    {
        entity.AuditInfo.CreatedAt = _timeProvider.GetUtcNowDateTime();
        entity.AuditInfo.CreatedByName = NoPIIName;
        entity.AuditInfo.CreatedById = string.Empty;
        entity.AuditInfo.CreatedByEmail = string.Empty;
    }

    public virtual void SetModified(IAuditedEntity? entity)
    {
        if (entity == null)
        {
            return;
        }

        entity.AuditInfo.ModifiedAt = _timeProvider.GetUtcNowDateTime();
        entity.AuditInfo.ModifiedById = UserId;
        entity.AuditInfo.ModifiedByName = UserName;
        entity.AuditInfo.ModifiedByEmail = UserEmail;
    }

    public virtual async Task<string?> GetSocialSecurityNumber(bool allowCache)
    {
        if (!allowCache)
        {
            var ssn = await _personServiceClient.GetPersonSsn(UserId);
            _ssnCache.TrySet(this, ssn);
            return ssn;
        }

        return _ssnCache.Get(this) ?? await GetSocialSecurityNumber(false);
    }

    public void Init(
        string userId,
        string userName,
        string userEmail,
        bool userEmailVerified,
        string userFirstName,
        string userLastName,
        DateTimeOffset authenticatedTime)
    {
        IsAuthenticated = true;
        UserId = userId;
        UserName = userName;
        UserEmail = userEmail;
        UserEmailVerified = userEmailVerified;
        UserFirstName = userFirstName;
        UserLastName = userLastName;
        AuthenticatedTime = authenticatedTime;
    }

    public void RequireEmail(string email)
    {
        if (!UserEmailVerified || !string.Equals(email, UserEmail, StringComparison.Ordinal))
        {
            throw new EmailDoesNotMatchException();
        }
    }
}
