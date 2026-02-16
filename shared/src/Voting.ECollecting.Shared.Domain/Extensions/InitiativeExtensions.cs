// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.Extensions;

public static class InitiativeExtensions
{
    public static void SetPeriodStates(this IEnumerable<InitiativeEntity> initiatives, DateOnly today)
    {
        foreach (var initiative in initiatives)
        {
            initiative.SetPeriodState(today);
        }
    }
}
