// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class DeleteInitiativeRequestTest : ProtoValidatorBaseTest<DeleteInitiativeRequest>
{
    protected override IEnumerable<DeleteInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.SecondFactorTransactionId = string.Empty);
        yield return NewValidRequest(x => x.SecondFactorTransactionId = "not a guid");
    }

    private static DeleteInitiativeRequest NewValidRequest(Action<DeleteInitiativeRequest>? customizer = null)
    {
        var request = new DeleteInitiativeRequest
        {
            InitiativeId = "0aeb26de-8678-4ace-a4c0-ed92b8650ae6",
            SecondFactorTransactionId = "c5ed1e1c-7532-4271-8ec3-961690756d43",
        };

        customizer?.Invoke(request);
        return request;
    }
}
