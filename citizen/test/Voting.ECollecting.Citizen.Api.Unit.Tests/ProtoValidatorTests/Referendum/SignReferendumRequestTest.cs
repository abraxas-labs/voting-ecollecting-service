// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class SignReferendumRequestTest : ProtoValidatorBaseTest<SignReferendumRequest>
{
    protected override IEnumerable<SignReferendumRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SignReferendumRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = "invalid-guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
    }

    private SignReferendumRequest NewValidRequest(Action<SignReferendumRequest>? customizer = null)
    {
        var request = new SignReferendumRequest
        {
            Id = "6db66400-c9d9-4cea-a30e-7b676ece928b",
        };

        customizer?.Invoke(request);
        return request;
    }
}
