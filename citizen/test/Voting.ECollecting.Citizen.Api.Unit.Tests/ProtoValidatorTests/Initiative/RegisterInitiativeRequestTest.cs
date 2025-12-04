// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class RegisterInitiativeRequestTest : ProtoValidatorBaseTest<RegisterInitiativeRequest>
{
    protected override IEnumerable<RegisterInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<RegisterInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static RegisterInitiativeRequest NewValidRequest(Action<RegisterInitiativeRequest>? customizer = null)
    {
        var request = new RegisterInitiativeRequest
        {
            Id = "3bd133f0-ac15-4a2f-b2c6-679f8623569c",
        };

        customizer?.Invoke(request);
        return request;
    }
}
