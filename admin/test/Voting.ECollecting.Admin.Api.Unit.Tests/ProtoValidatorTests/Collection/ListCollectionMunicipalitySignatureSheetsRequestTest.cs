// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ListCollectionMunicipalitySignatureSheetsRequestTest : ProtoValidatorBaseTest<ListCollectionMunicipalitySignatureSheetsRequest>
{
    protected override IEnumerable<ListCollectionMunicipalitySignatureSheetsRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(8));
    }

    protected override IEnumerable<ListCollectionMunicipalitySignatureSheetsRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(9));
        yield return NewValidRequest(x => x.Bfs = string.Empty);
    }

    private static ListCollectionMunicipalitySignatureSheetsRequest NewValidRequest(Action<ListCollectionMunicipalitySignatureSheetsRequest>? customizer = null)
    {
        var request = new ListCollectionMunicipalitySignatureSheetsRequest
        {
            CollectionId = "b59c947a-66be-4855-8c55-eef9d1b5df9e",
            Bfs = "3203",
        };

        customizer?.Invoke(request);
        return request;
    }
}
