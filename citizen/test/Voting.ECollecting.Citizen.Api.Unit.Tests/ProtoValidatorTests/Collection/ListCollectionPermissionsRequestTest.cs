// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

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
            CollectionId = "37eee602-8139-44d4-bcb5-33511d229f9a",
        };

        customizer?.Invoke(request);
        return request;
    }
}
