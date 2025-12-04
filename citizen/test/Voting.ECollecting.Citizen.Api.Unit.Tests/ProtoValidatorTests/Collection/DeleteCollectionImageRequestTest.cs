// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteCollectionImageRequestTest : ProtoValidatorBaseTest<DeleteCollectionImageRequest>
{
    protected override IEnumerable<DeleteCollectionImageRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteCollectionImageRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
    }

    private static DeleteCollectionImageRequest NewValidRequest(Action<DeleteCollectionImageRequest>? customizer = null)
    {
        var request = new DeleteCollectionImageRequest
        {
            CollectionId = "06e560ed-8525-474d-935a-aa0f36227d06",
        };

        customizer?.Invoke(request);
        return request;
    }
}
