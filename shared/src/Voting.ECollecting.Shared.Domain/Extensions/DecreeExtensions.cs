// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.Extensions;

public static class DecreeExtensions
{
    public static void SetPeriodStates(this IEnumerable<DecreeEntity> decrees, DateTime utcNow)
    {
        foreach (var decree in decrees)
        {
            decree.SetPeriodState(utcNow);
        }
    }
}
