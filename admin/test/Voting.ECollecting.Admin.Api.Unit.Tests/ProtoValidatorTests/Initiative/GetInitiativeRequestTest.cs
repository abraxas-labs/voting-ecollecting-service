// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class GetInitiativeRequestTest : ProtoValidatorBaseTest<GetInitiativeRequest>
{
    protected override IEnumerable<GetInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<GetInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static GetInitiativeRequest NewValidRequest(Action<GetInitiativeRequest>? customizer = null)
    {
        var request = new GetInitiativeRequest
        {
            Id = "3825f6a2-78ad-4c8e-ba96-5ae2dfacd4de",
        };

        customizer?.Invoke(request);
        return request;
    }
}
