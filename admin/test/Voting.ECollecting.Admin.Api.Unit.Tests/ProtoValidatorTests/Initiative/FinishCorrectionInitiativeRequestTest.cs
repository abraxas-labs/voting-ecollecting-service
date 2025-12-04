// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class FinishCorrectionInitiativeRequestTest : ProtoValidatorBaseTest<FinishCorrectionInitiativeRequest>
{
    protected override IEnumerable<FinishCorrectionInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<FinishCorrectionInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static FinishCorrectionInitiativeRequest NewValidRequest(Action<FinishCorrectionInitiativeRequest>? customizer = null)
    {
        var request = new FinishCorrectionInitiativeRequest
        {
            Id = "65179ad6-8707-44ca-bff5-9376f91620cd",
        };

        customizer?.Invoke(request);
        return request;
    }
}
