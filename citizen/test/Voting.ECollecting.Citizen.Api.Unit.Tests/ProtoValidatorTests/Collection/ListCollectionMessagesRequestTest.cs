// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ListCollectionMessagesRequestTest : ProtoValidatorBaseTest<ListCollectionMessagesRequest>
{
    protected override IEnumerable<ListCollectionMessagesRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ListCollectionMessagesRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
    }

    private static ListCollectionMessagesRequest NewValidRequest(Action<ListCollectionMessagesRequest>? customizer = null)
    {
        var request = new ListCollectionMessagesRequest
        {
            CollectionId = "bf5fa458-a7fd-45c2-b59a-3c0f2117addc",
        };

        customizer?.Invoke(request);
        return request;
    }
}
