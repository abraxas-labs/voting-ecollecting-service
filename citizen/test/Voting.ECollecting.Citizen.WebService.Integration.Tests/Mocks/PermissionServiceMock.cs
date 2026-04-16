// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Adapter.ELogin;
using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;

public class PermissionServiceMock : PermissionService
{
    private static int _counter;
    private static bool _hasTimestampIncrement;
    private string? _userSsn;

    public PermissionServiceMock(TimeProvider timeProvider, SocialSecurityNumberCache cache, PersonServiceClient personServiceClient)
        : base(timeProvider, cache, personServiceClient)
    {
    }

    // Needed to have a deterministic sort for snapshots (e.g. audit trail)
    public static bool HasTimestampIncrement
    {
        set
        {
            if (value == _hasTimestampIncrement)
            {
                return;
            }

            _hasTimestampIncrement = value;
            _counter = 0;
        }
    }

    public override Task<string?> GetSocialSecurityNumber(bool allowCache) => Task.FromResult(_userSsn);

    public override void SetCreated(IAuditedEntity entity)
    {
        base.SetCreated(entity);

        if (_hasTimestampIncrement)
        {
            entity.AuditInfo.CreatedAt = entity.AuditInfo.CreatedAt.AddSeconds(++_counter);
        }
    }

    public override void SetModified(IAuditedEntity? entity)
    {
        base.SetModified(entity);

        if (_hasTimestampIncrement && entity != null)
        {
            entity.AuditInfo.ModifiedAt = entity.AuditInfo.ModifiedAt!.Value.AddSeconds(++_counter);
        }
    }

    public void SetSsn(string? userSsn)
        => _userSsn = userSsn;
}
