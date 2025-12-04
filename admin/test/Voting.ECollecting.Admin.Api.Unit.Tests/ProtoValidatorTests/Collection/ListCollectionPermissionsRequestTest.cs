// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ListCollectionPermissionsRequestTest : ProtoValidatorBaseTest<ListCollectionPermissionsRequest>
{
    protected override IEnumerable<ListCollectionPermissionsRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ListCollectionPermissionsRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
    }

    private static ListCollectionPermissionsRequest NewValidRequest(Action<ListCollectionPermissionsRequest>? customizer = null)
    {
        var request = new ListCollectionPermissionsRequest
        {
            CollectionId = "54345774-02dc-4aa6-8aac-48ff177bbbf9",
        };

        customizer?.Invoke(request);
        return request;
    }
}
