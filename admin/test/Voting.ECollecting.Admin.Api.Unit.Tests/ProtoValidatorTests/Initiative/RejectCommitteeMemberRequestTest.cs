// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class RejectCommitteeMemberRequestTest : ProtoValidatorBaseTest<RejectCommitteeMemberRequest>
{
    protected override IEnumerable<RejectCommitteeMemberRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<RejectCommitteeMemberRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static RejectCommitteeMemberRequest NewValidRequest(Action<RejectCommitteeMemberRequest>? customizer = null)
    {
        var request = new RejectCommitteeMemberRequest
        {
            InitiativeId = "da5d5816-7b72-4235-8b11-b70c63119f1a",
            Id = "a716f838-43c4-40d8-9168-90c7583412d5",
        };

        customizer?.Invoke(request);
        return request;
    }
}
