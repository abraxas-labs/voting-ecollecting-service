// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class CreateReferendumRequestTest : ProtoValidatorBaseTest<CreateReferendumRequest>
{
    protected override IEnumerable<CreateReferendumRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(200));
    }

    protected override IEnumerable<CreateReferendumRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.DecreeId = string.Empty);
        yield return NewValidRequest(x => x.DecreeId = "not a guid");
        yield return NewValidRequest(x => x.Description = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(201));
        yield return NewValidRequest(x => x.Description = "Te\nst");
        yield return NewValidRequest(x => x.Address = null);
    }

    private static CreateReferendumRequest NewValidRequest(Action<CreateReferendumRequest>? customizer = null)
    {
        var request = new CreateReferendumRequest
        {
            DecreeId = "03a8221e-43f4-4c77-bf78-407a4a17db54",
            Description = "Sammlung gegen das Abwassergesetz",
            Address = CollectionAddressTest.NewValidRequest(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
