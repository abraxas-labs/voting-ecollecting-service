// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class SetReferendumInPreparationRequestTest : ProtoValidatorBaseTest<SetReferendumInPreparationRequest>
{
    protected override IEnumerable<SetReferendumInPreparationRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SetReferendumInPreparationRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.SecureIdNumber = string.Empty);
        yield return NewValidRequest(x => x.SecureIdNumber = "AAAAAAAAAAAAA");
        yield return NewValidRequest(x => x.SecureIdNumber = "AAAAAAAAAAA");
        yield return NewValidRequest(x => x.SecureIdNumber = "AAAAAAAAAAA\n");
    }

    private SetReferendumInPreparationRequest NewValidRequest(Action<SetReferendumInPreparationRequest>? customizer = null)
    {
        var request = new SetReferendumInPreparationRequest
        {
            SecureIdNumber = "AAAAAAAAAAAA",
        };

        customizer?.Invoke(request);
        return request;
    }
}
