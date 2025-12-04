// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

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
            Id = "3f83ae68-f6a9-4b23-a0a7-fb1100f031a7",
            IncludeCommitteeDescription = true,
            IncludeIsSigned = true,
        };

        customizer?.Invoke(request);
        return request;
    }
}
