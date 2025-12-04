// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class VerifyCommitteeMemberRequestTest : ProtoValidatorBaseTest<VerifyCommitteeMemberRequest>
{
    protected override IEnumerable<VerifyCommitteeMemberRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<VerifyCommitteeMemberRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static VerifyCommitteeMemberRequest NewValidRequest(Action<VerifyCommitteeMemberRequest>? customizer = null)
    {
        var request = new VerifyCommitteeMemberRequest
        {
            InitiativeId = "97b1343c-008c-4f4d-a9b4-da7f2867fcba",
            Id = "70499c80-f42f-4773-8e27-16031b7e7ce3",
        };

        customizer?.Invoke(request);
        return request;
    }
}
