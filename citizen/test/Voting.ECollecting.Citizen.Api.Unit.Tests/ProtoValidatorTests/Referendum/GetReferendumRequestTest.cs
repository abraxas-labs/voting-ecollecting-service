// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class GetReferendumRequestTest : ProtoValidatorBaseTest<GetReferendumRequest>
{
    protected override IEnumerable<GetReferendumRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<GetReferendumRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = "invalid-guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
    }

    private GetReferendumRequest NewValidRequest(Action<GetReferendumRequest>? customizer = null)
    {
        var request = new GetReferendumRequest
        {
            Id = "b9a5b3b6-2b49-4d76-a5ce-894c31fd4cdc",
        };

        customizer?.Invoke(request);
        return request;
    }
}
