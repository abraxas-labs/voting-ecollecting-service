// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class WithdrawCollectionRequestTest : ProtoValidatorBaseTest<WithdrawCollectionRequest>
{
    protected override IEnumerable<WithdrawCollectionRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<WithdrawCollectionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static WithdrawCollectionRequest NewValidRequest(Action<WithdrawCollectionRequest>? customizer = null)
    {
        var request = new WithdrawCollectionRequest
        {
            Id = "3f83ae68-f6a9-4b23-a0a7-fb1100f031a7",
        };

        customizer?.Invoke(request);
        return request;
    }
}
