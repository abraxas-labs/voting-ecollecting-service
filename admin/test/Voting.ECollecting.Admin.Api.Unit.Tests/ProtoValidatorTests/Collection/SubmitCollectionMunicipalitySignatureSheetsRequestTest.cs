// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class SubmitCollectionMunicipalitySignatureSheetsRequestTest : ProtoValidatorBaseTest<SubmitCollectionMunicipalitySignatureSheetsRequest>
{
    protected override IEnumerable<SubmitCollectionMunicipalitySignatureSheetsRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(8));
    }

    protected override IEnumerable<SubmitCollectionMunicipalitySignatureSheetsRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(9));
        yield return NewValidRequest(x => x.Bfs = string.Empty);
    }

    private static SubmitCollectionMunicipalitySignatureSheetsRequest NewValidRequest(Action<SubmitCollectionMunicipalitySignatureSheetsRequest>? customizer = null)
    {
        var request = new SubmitCollectionMunicipalitySignatureSheetsRequest
        {
            CollectionId = "7ce44bbf-75c6-48af-933d-42547517819d",
            Bfs = "3203",
        };

        customizer?.Invoke(request);
        return request;
    }
}
