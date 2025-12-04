// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Queries;

namespace Voting.ECollecting.Admin.Domain.Queries;

public static class CollectionSignatureSheetQueries
{
    public static IQueryable<CollectionSignatureSheetEntity> OrderBy(
        this IQueryable<CollectionSignatureSheetEntity> q,
        CollectionSignatureSheetSort sort,
        SortDirection direction)
    {
        return sort switch
        {
            CollectionSignatureSheetSort.Unspecified => q.OrderBy(x => x.Number, direction),
            CollectionSignatureSheetSort.Number => q.OrderBy(x => x.Number, direction),
            CollectionSignatureSheetSort.Date => q.OrderBy(x => x.ReceivedAt, direction),
            CollectionSignatureSheetSort.ModifiedAt => q.OrderBy(x => x.AuditInfo.ModifiedAt, direction),
            CollectionSignatureSheetSort.AttestedAt => q.OrderBy(x => x.AttestedAt, direction),
            CollectionSignatureSheetSort.CountTotal => q.OrderBy(x => x.Count.Valid + x.Count.Invalid, direction),
            CollectionSignatureSheetSort.CountValid => q.OrderBy(x => x.Count.Valid, direction),
            CollectionSignatureSheetSort.CountInvalid => q.OrderBy(x => x.Count.Invalid, direction),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null),
        };
    }
}
