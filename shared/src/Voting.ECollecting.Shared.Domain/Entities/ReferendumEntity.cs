// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities;

public class ReferendumEntity : CollectionBaseEntity
{
    public string MembersCommittee { get; set; } = string.Empty;

    public Guid? DecreeId { get; set; }

    /// <summary>
    /// Gets or sets the optional reference to the decree. Only set if the collection is for a referendum.
    /// </summary>
    public DecreeEntity? Decree { get; set; }

    public override void SetPeriodState(DateOnly today)
    {
        base.SetPeriodState(today);
        Decree?.SetPeriodState(today);
    }
}
