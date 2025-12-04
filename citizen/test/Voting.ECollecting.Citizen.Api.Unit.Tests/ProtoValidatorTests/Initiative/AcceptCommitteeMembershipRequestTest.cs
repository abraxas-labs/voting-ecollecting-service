// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Common;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class AcceptCommitteeMembershipRequestTest : ProtoValidatorBaseTest<AcceptCommitteeMembershipRequest>
{
    protected override IEnumerable<AcceptCommitteeMembershipRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<AcceptCommitteeMembershipRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Token = string.Empty);
        yield return NewValidRequest(x => x.Token = "not a guid");
    }

    private static AcceptCommitteeMembershipRequest NewValidRequest(Action<AcceptCommitteeMembershipRequest>? customizer = null)
    {
        var request = new AcceptCommitteeMembershipRequest
        {
            Token = UrlToken.New(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
