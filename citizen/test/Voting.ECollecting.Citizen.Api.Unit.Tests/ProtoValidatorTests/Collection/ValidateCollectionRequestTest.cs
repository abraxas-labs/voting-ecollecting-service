// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ValidateCollectionRequestTest : ProtoValidatorBaseTest<ValidateCollectionRequest>
{
    protected override IEnumerable<ValidateCollectionRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ValidateCollectionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static ValidateCollectionRequest NewValidRequest(Action<ValidateCollectionRequest>? customizer = null)
    {
        var request = new ValidateCollectionRequest
        {
            Id = "6a5bbad5-2156-40c8-a5e0-8852186d83bc",
        };

        customizer?.Invoke(request);
        return request;
    }
}
