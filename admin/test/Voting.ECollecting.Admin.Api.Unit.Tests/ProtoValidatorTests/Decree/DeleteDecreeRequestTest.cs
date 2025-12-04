// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Decree;

public class DeleteDecreeRequestTest : ProtoValidatorBaseTest<DeleteDecreeRequest>
{
    protected override IEnumerable<DeleteDecreeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteDecreeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.DecreeId = string.Empty);
        yield return NewValidRequest(x => x.DecreeId = "not a guid");
        yield return NewValidRequest(x => x.SecondFactorTransactionId = string.Empty);
        yield return NewValidRequest(x => x.SecondFactorTransactionId = "not a guid");
    }

    private static DeleteDecreeRequest NewValidRequest(Action<DeleteDecreeRequest>? customizer = null)
    {
        var request = new DeleteDecreeRequest
        {
            DecreeId = "0aeb26de-8678-4ace-a4c0-ed92b8650ae6",
            SecondFactorTransactionId = "c5ed1e1c-7532-4271-8ec3-961690756d43",
        };

        customizer?.Invoke(request);
        return request;
    }
}
