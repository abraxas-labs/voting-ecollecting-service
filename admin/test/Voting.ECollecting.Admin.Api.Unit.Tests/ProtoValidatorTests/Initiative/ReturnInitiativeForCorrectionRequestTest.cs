// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class ReturnInitiativeForCorrectionRequestTest : ProtoValidatorBaseTest<ReturnInitiativeForCorrectionRequest>
{
    protected override IEnumerable<ReturnInitiativeForCorrectionRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.LockedFields = null);
    }

    protected override IEnumerable<ReturnInitiativeForCorrectionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static ReturnInitiativeForCorrectionRequest NewValidRequest(Action<ReturnInitiativeForCorrectionRequest>? customizer = null)
    {
        var request = new ReturnInitiativeForCorrectionRequest
        {
            Id = "65179ad6-8707-44ca-bff5-9376f91620cd",
            LockedFields = new InitiativeLockedFields
            {
                Description = false,
                Wording = false,
                CommitteeMembers = true,
            },
        };

        customizer?.Invoke(request);
        return request;
    }
}
