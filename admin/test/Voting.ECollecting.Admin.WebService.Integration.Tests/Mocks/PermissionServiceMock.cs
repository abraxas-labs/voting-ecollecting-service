// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Adapter.VotingIam;
using Voting.ECollecting.Admin.Adapter.VotingIam.Configuration;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Iam.Store;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;

public class PermissionServiceMock : PermissionService
{
    private static int _counter;
    private static bool _hasTimestampIncrement;

    public PermissionServiceMock(
        IAuth auth,
        IAuthStore authStore,
        ILogger<PermissionService> logger,
        VotingIamConfig config,
        TimeProvider timeProvider)
        : base(auth, authStore, logger, config, timeProvider)
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

    public override void SetCreated(IAuditedEntity entity)
    {
        base.SetCreated(entity);

        if (_hasTimestampIncrement)
        {
            entity.AuditInfo.CreatedAt = entity.AuditInfo.CreatedAt.AddSeconds(++_counter);
        }
    }

    public override void SetModified(IAuditedEntity entity)
    {
        base.SetModified(entity);

        if (_hasTimestampIncrement)
        {
            entity.AuditInfo.ModifiedAt = entity.AuditInfo.ModifiedAt!.Value.AddSeconds(++_counter);
        }
    }
}
