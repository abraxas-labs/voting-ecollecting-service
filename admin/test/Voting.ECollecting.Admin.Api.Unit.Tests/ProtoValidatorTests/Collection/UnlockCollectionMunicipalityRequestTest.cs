// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class UnlockCollectionMunicipalityRequestTest : ProtoValidatorBaseTest<UnlockCollectionMunicipalityRequest>
{
    protected override IEnumerable<UnlockCollectionMunicipalityRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(8));
    }

    protected override IEnumerable<UnlockCollectionMunicipalityRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(9));
        yield return NewValidRequest(x => x.Bfs = string.Empty);
    }

    private static UnlockCollectionMunicipalityRequest NewValidRequest(Action<UnlockCollectionMunicipalityRequest>? customizer = null)
    {
        var request = new UnlockCollectionMunicipalityRequest
        {
            CollectionId = "213d026d-ed22-4c5d-9364-eaf01471007f",
            Bfs = "3203",
        };

        customizer?.Invoke(request);
        return request;
    }
}
