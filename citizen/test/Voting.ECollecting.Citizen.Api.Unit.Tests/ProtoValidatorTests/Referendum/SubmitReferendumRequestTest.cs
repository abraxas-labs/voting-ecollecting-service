// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class SubmitReferendumRequestTest : ProtoValidatorBaseTest<SubmitReferendumRequest>
{
    protected override IEnumerable<SubmitReferendumRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SubmitReferendumRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static SubmitReferendumRequest NewValidRequest(Action<SubmitReferendumRequest>? customizer = null)
    {
        var request = new SubmitReferendumRequest
        {
            Id = "f62e67fc-91f9-4ec5-9f8f-60bc322b63fa",
        };

        customizer?.Invoke(request);
        return request;
    }
}
