// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class GetInitiativeCommitteeRequestTest : ProtoValidatorBaseTest<GetInitiativeCommitteeRequest>
{
    protected override IEnumerable<GetInitiativeCommitteeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<GetInitiativeCommitteeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static GetInitiativeCommitteeRequest NewValidRequest(Action<GetInitiativeCommitteeRequest>? customizer = null)
    {
        var request = new GetInitiativeCommitteeRequest
        {
            Id = "c3005b7c-59d1-4018-926b-355bb0850804",
        };

        customizer?.Invoke(request);
        return request;
    }
}
