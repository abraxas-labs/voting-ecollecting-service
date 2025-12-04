// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class ResendCommitteeMemberInvitationRequestTest : ProtoValidatorBaseTest<ResendCommitteeMemberInvitationRequest>
{
    protected override IEnumerable<ResendCommitteeMemberInvitationRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ResendCommitteeMemberInvitationRequest> NotOkMessages()
    {
        yield return NewValidRequest(r => r.Id = string.Empty);
        yield return NewValidRequest(r => r.Id = "foo");
        yield return NewValidRequest(r => r.InitiativeId = string.Empty);
        yield return NewValidRequest(r => r.InitiativeId = "foo");
    }

    private ResendCommitteeMemberInvitationRequest NewValidRequest(Action<ResendCommitteeMemberInvitationRequest>? customizer = null)
    {
        var request = new ResendCommitteeMemberInvitationRequest
        {
            Id = "74d415cd-916c-4571-9c73-517b94b56cec",
            InitiativeId = "751b2313-e7d3-4b98-bc7e-56097d8b4e9a",
        };

        customizer?.Invoke(request);
        return request;
    }
}
