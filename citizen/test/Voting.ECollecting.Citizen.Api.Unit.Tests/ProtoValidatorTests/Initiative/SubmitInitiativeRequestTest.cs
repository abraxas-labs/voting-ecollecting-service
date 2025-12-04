// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class SubmitInitiativeRequestTest : ProtoValidatorBaseTest<SubmitInitiativeRequest>
{
    protected override IEnumerable<SubmitInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SubmitInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static SubmitInitiativeRequest NewValidRequest(Action<SubmitInitiativeRequest>? customizer = null)
    {
        var request = new SubmitInitiativeRequest
        {
            Id = "922e574b-8770-4f6a-9aa0-b9a2b09efaf5",
        };

        customizer?.Invoke(request);
        return request;
    }
}
