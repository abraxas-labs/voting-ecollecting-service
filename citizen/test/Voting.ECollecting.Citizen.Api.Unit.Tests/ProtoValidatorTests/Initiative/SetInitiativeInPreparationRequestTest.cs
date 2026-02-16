// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class SetInitiativeInPreparationRequestTest : ProtoValidatorBaseTest<SetInitiativeInPreparationRequest>
{
    protected override IEnumerable<SetInitiativeInPreparationRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SetInitiativeInPreparationRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.SecureIdNumber = string.Empty);
        yield return NewValidRequest(x => x.SecureIdNumber = "AAAAAAAAAAAAA");
        yield return NewValidRequest(x => x.SecureIdNumber = "AAAAAAAAAAA");
        yield return NewValidRequest(x => x.SecureIdNumber = "AAAAAAAAAAA\n");
    }

    private static SetInitiativeInPreparationRequest NewValidRequest(Action<SetInitiativeInPreparationRequest>? customizer = null)
    {
        var request = new SetInitiativeInPreparationRequest
        {
            SecureIdNumber = "AAAAAAAAAAAA",
        };

        customizer?.Invoke(request);
        return request;
    }
}
