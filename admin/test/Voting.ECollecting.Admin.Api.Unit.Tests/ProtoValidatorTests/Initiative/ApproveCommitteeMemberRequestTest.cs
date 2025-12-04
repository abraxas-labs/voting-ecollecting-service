// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class ApproveCommitteeMemberRequestTest : ProtoValidatorBaseTest<ApproveCommitteeMemberRequest>
{
    protected override IEnumerable<ApproveCommitteeMemberRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ApproveCommitteeMemberRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static ApproveCommitteeMemberRequest NewValidRequest(Action<ApproveCommitteeMemberRequest>? customizer = null)
    {
        var request = new ApproveCommitteeMemberRequest
        {
            InitiativeId = "7b43d9b8-fabc-4f02-ad16-9b030153d5e6",
            Id = "cc73fd65-37a7-492d-9df6-1ffd27908e61",
        };

        customizer?.Invoke(request);
        return request;
    }
}
