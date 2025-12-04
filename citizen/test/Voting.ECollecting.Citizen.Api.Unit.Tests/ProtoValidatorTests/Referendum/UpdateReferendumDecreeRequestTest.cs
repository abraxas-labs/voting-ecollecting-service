// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class UpdateReferendumDecreeRequestTest : ProtoValidatorBaseTest<UpdateReferendumDecreeRequest>
{
    protected override IEnumerable<UpdateReferendumDecreeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<UpdateReferendumDecreeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = "invalid-guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.DecreeId = "invalid-guid");
        yield return NewValidRequest(x => x.DecreeId = string.Empty);
    }

    private UpdateReferendumDecreeRequest NewValidRequest(Action<UpdateReferendumDecreeRequest>? customizer = null)
    {
        var request = new UpdateReferendumDecreeRequest
        {
            Id = "43796e0a-67ce-4760-b609-057d92f8c282",
            DecreeId = "2ad71425-f2fb-4d4c-821e-59b60f6e6c65",
        };

        customizer?.Invoke(request);
        return request;
    }
}
