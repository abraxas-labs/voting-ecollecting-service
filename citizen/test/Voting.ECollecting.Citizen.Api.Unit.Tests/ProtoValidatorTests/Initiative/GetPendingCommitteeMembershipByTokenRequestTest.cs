// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Common;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class GetPendingCommitteeMembershipByTokenRequestTest : ProtoValidatorBaseTest<GetPendingCommitteeMembershipByTokenRequest>
{
    protected override IEnumerable<GetPendingCommitteeMembershipByTokenRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<GetPendingCommitteeMembershipByTokenRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Token = string.Empty);
        yield return NewValidRequest(x => x.Token = "not a guid");
    }

    private static GetPendingCommitteeMembershipByTokenRequest NewValidRequest(Action<GetPendingCommitteeMembershipByTokenRequest>? customizer = null)
    {
        var request = new GetPendingCommitteeMembershipByTokenRequest
        {
            Token = UrlToken.New(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
