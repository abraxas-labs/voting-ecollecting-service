// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Enums;
using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ListSignatureSheetsRequestTest : ProtoValidatorBaseTest<ListSignatureSheetsRequest>
{
    protected override IEnumerable<ListSignatureSheetsRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Pageable = null);
        yield return NewValidRequest(x => x.Sort = ListSignatureSheetsSort.Unspecified);
        yield return NewValidRequest(x => x.SortDirection = SortDirection.Unspecified);
        yield return NewValidRequest(x => x.AttestedAts.Clear());
        yield return NewValidRequest(x => x.States.Clear());
    }

    protected override IEnumerable<ListSignatureSheetsRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.Pageable.PageSize = 101);
        yield return NewValidRequest(x => x.Pageable.Page = 0);
        yield return NewValidRequest(x => x.States.Add(CollectionSignatureSheetState.Unspecified));
        yield return NewValidRequest(x => x.States.Add((CollectionSignatureSheetState)1000));
        yield return NewValidRequest(x => x.Sort = (ListSignatureSheetsSort)1000);
        yield return NewValidRequest(x => x.SortDirection = (SortDirection)1000);
    }

    private static ListSignatureSheetsRequest NewValidRequest(
        Action<ListSignatureSheetsRequest>? customizer = null)
    {
        var request = new ListSignatureSheetsRequest
        {
            CollectionId = "96095807-d16d-4eca-a4f6-e92ecb1b31d5",
            Pageable = new Pageable
            {
                Page = 1,
                PageSize = 10,
            },
            Sort = ListSignatureSheetsSort.AttestedAt,
            SortDirection = SortDirection.Ascending,
            States = { CollectionSignatureSheetState.Attested },
            AttestedAts =
            {
                Timestamp.FromDateTime(new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc)),
                Timestamp.FromDateTime(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc)),
            },
        };

        customizer?.Invoke(request);
        return request;
    }
}
