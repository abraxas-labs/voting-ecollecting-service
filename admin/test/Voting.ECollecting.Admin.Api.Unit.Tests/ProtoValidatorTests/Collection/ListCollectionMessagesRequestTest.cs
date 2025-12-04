// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

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
            CollectionId = "96095807-d16d-4eca-a4f6-e92ecb1b31d5",
        };

        customizer?.Invoke(request);
        return request;
    }
}
