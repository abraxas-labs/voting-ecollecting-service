// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteWithdrawnCollectionRequestTest : ProtoValidatorBaseTest<DeleteWithdrawnCollectionRequest>
{
    protected override IEnumerable<DeleteWithdrawnCollectionRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteWithdrawnCollectionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
    }

    private static DeleteWithdrawnCollectionRequest NewValidRequest(Action<DeleteWithdrawnCollectionRequest>? customizer = null)
    {
        var request = new DeleteWithdrawnCollectionRequest
        {
            CollectionId = "0c8a384f-5bb4-428c-8066-412cd7746c9e",
        };

        customizer?.Invoke(request);
        return request;
    }
}
