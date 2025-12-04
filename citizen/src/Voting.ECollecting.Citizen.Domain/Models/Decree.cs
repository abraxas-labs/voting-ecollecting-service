// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Citizen.Domain.Models;

public class Decree : DecreeEntity
{
    public DecreeUserPermissions? UserPermissions { get; set; }

    public List<Referendum> Referendums { get; set; } = [];

    public NullableCollectionCount? AttestedCollectionCount
    {
        get
        {
            if (Referendums.Count == 0 || Referendums.All(r => r.AttestedCollectionCount == null))
            {
                return null;
            }

            return Referendums
                .Where(x => x.AttestedCollectionCount != null)
                .Aggregate(new NullableCollectionCount(), (c, r) =>
                {
                    c.ElectronicCitizenCount += r.AttestedCollectionCount!.ElectronicCitizenCount;

                    if (r.AttestedCollectionCount.TotalCitizenCount.HasValue)
                    {
                        c.TotalCitizenCount ??= 0;
                        c.TotalCitizenCount += r.AttestedCollectionCount.TotalCitizenCount;
                    }

                    return c;
                });
        }
    }
}
