// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class SignInitiativeRequestTest : ProtoValidatorBaseTest<SignInitiativeRequest>
{
    protected override IEnumerable<SignInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SignInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static SignInitiativeRequest NewValidRequest(Action<SignInitiativeRequest>? customizer = null)
    {
        var request = new SignInitiativeRequest
        {
            Id = "63275a55-72f0-46de-bf04-a0dc3f8dada0",
        };

        customizer?.Invoke(request);
        return request;
    }
}
