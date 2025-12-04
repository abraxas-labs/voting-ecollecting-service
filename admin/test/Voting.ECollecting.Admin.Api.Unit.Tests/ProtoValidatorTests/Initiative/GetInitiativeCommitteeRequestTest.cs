// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

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
            Id = "87f57e14-e266-4538-a426-b0702ca7a128",
        };

        customizer?.Invoke(request);
        return request;
    }
}
