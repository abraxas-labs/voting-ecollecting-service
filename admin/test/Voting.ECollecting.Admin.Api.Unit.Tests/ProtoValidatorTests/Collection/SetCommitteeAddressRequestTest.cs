// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class SetCommitteeAddressRequestTest : ProtoValidatorBaseTest<SetCommitteeAddressRequest>
{
    protected override IEnumerable<SetCommitteeAddressRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SetCommitteeAddressRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a uuid");
        yield return NewValidRequest(x => x.Address = null);
        yield return NewValidRequest(x => x.Address = CollectionAddressTest.NewInvalidRequest());
    }

    private SetCommitteeAddressRequest NewValidRequest(Action<SetCommitteeAddressRequest>? customizer = null)
    {
        var req = new SetCommitteeAddressRequest
        {
            CollectionId = "1370889e-47e8-4bd7-b33b-1623153a0583",
            Address = CollectionAddressTest.NewValidRequest(),
        };

        customizer?.Invoke(req);
        return req;
    }
}
