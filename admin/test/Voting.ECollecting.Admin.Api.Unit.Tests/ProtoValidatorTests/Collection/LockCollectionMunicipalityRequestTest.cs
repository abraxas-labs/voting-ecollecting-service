// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class LockCollectionMunicipalityRequestTest : ProtoValidatorBaseTest<LockCollectionMunicipalityRequest>
{
    protected override IEnumerable<LockCollectionMunicipalityRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(8));
    }

    protected override IEnumerable<LockCollectionMunicipalityRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(9));
        yield return NewValidRequest(x => x.Bfs = string.Empty);
    }

    private static LockCollectionMunicipalityRequest NewValidRequest(Action<LockCollectionMunicipalityRequest>? customizer = null)
    {
        var request = new LockCollectionMunicipalityRequest
        {
            CollectionId = "5bf1c64e-cbeb-4028-ad57-66bc9d6a78fe",
            Bfs = "3203",
        };

        customizer?.Invoke(request);
        return request;
    }
}
