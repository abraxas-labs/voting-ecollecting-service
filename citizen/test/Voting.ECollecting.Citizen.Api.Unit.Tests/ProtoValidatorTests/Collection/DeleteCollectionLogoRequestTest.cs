// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteCollectionLogoRequestTest : ProtoValidatorBaseTest<DeleteCollectionLogoRequest>
{
    protected override IEnumerable<DeleteCollectionLogoRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteCollectionLogoRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
    }

    private static DeleteCollectionLogoRequest NewValidRequest(Action<DeleteCollectionLogoRequest>? customizer = null)
    {
        var request = new DeleteCollectionLogoRequest
        {
            CollectionId = "7f3d11da-9382-4369-a303-0e61ef3085a8",
        };

        customizer?.Invoke(request);
        return request;
    }
}
