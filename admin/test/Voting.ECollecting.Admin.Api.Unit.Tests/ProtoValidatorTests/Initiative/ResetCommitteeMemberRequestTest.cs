// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class ResetCommitteeMemberRequestTest : ProtoValidatorBaseTest<ResetCommitteeMemberRequest>
{
    protected override IEnumerable<ResetCommitteeMemberRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ResetCommitteeMemberRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static ResetCommitteeMemberRequest NewValidRequest(Action<ResetCommitteeMemberRequest>? customizer = null)
    {
        var request = new ResetCommitteeMemberRequest
        {
            InitiativeId = "07d5bd71-b3a5-4a05-97c5-0171a3e56787",
            Id = "8cbf8765-1d62-4503-a287-c025b4743e9e",
        };

        customizer?.Invoke(request);
        return request;
    }
}
