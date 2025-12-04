// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class DeleteCommitteeMemberRequestTest : ProtoValidatorBaseTest<DeleteCommitteeMemberRequest>
{
    protected override IEnumerable<DeleteCommitteeMemberRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteCommitteeMemberRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = "invalid-guid");
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.Id = "invalid-guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
    }

    private DeleteCommitteeMemberRequest NewValidRequest(Action<DeleteCommitteeMemberRequest>? customizer = null)
    {
        var request = new DeleteCommitteeMemberRequest
        {
            InitiativeId = "bcc3b914-58ed-448b-a313-1f1c53d1c124",
            Id = "7cfe2a51-4a78-40da-93b2-1463fc4c8588",
        };

        customizer?.Invoke(request);
        return request;
    }
}
