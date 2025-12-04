// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ListCollectionMunicipalitiesRequestTest : ProtoValidatorBaseTest<ListCollectionMunicipalitiesRequest>
{
    protected override IEnumerable<ListCollectionMunicipalitiesRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ListCollectionMunicipalitiesRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
    }

    private static ListCollectionMunicipalitiesRequest NewValidRequest(Action<ListCollectionMunicipalitiesRequest>? customizer = null)
    {
        var request = new ListCollectionMunicipalitiesRequest
        {
            CollectionId = "caca2c44-4899-47d5-a5fb-1fb11504d8d3",
        };

        customizer?.Invoke(request);
        return request;
    }
}
