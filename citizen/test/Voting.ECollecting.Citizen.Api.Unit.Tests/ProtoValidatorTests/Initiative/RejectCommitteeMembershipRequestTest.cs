// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Common;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class RejectCommitteeMembershipRequestTest : ProtoValidatorBaseTest<RejectCommitteeMembershipRequest>
{
    protected override IEnumerable<RejectCommitteeMembershipRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<RejectCommitteeMembershipRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Token = string.Empty);
        yield return NewValidRequest(x => x.Token = "not a guid");
    }

    private static RejectCommitteeMembershipRequest NewValidRequest(Action<RejectCommitteeMembershipRequest>? customizer = null)
    {
        var request = new RejectCommitteeMembershipRequest
        {
            Token = UrlToken.New(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
