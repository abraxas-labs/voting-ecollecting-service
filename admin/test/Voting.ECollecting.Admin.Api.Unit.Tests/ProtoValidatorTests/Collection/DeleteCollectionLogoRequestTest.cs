// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteCollectionLogoRequestTest : ProtoValidatorBaseTest<DeleteCollectionLogoRequest>
{
    protected override IEnumerable<DeleteCollectionLogoRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Referendum);
    }

    protected override IEnumerable<DeleteCollectionLogoRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Unspecified);
        yield return NewValidRequest(x => x.CollectionType = (CollectionType)(-1));
    }

    private static DeleteCollectionLogoRequest NewValidRequest(Action<DeleteCollectionLogoRequest>? customizer = null)
    {
        var request = new DeleteCollectionLogoRequest
        {
            CollectionId = "b716a3c3-34f6-4fc6-a142-5a598183d166",
            CollectionType = CollectionType.Initiative,
        };

        customizer?.Invoke(request);
        return request;
    }
}
