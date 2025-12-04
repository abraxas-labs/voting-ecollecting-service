// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteCollectionImageRequestTest : ProtoValidatorBaseTest<DeleteCollectionImageRequest>
{
    protected override IEnumerable<DeleteCollectionImageRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Referendum);
    }

    protected override IEnumerable<DeleteCollectionImageRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Unspecified);
        yield return NewValidRequest(x => x.CollectionType = (CollectionType)(-1));
    }

    private static DeleteCollectionImageRequest NewValidRequest(Action<DeleteCollectionImageRequest>? customizer = null)
    {
        var request = new DeleteCollectionImageRequest
        {
            CollectionId = "05f8dddd-4342-47fd-8ab8-6b6bbfd5e7bd",
            CollectionType = CollectionType.Initiative,
        };

        customizer?.Invoke(request);
        return request;
    }
}
