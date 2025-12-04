// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class UpdateInitiativeRequestTest : ProtoValidatorBaseTest<UpdateInitiativeRequest>
{
    protected override IEnumerable<UpdateInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Address = null);
        yield return NewValidRequest(x => x.SubTypeId = string.Empty);
        yield return NewValidRequest(x => x.Wording = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(200));
        yield return NewValidRequest(x => x.Wording = RandomStringUtil.GenerateComplexMultiLineText(10_000));
    }

    protected override IEnumerable<UpdateInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Address = CollectionAddressTest.NewInvalidRequest());
        yield return NewValidRequest(x => x.SubTypeId = "foobar");
        yield return NewValidRequest(x => x.Description = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(201));
        yield return NewValidRequest(x => x.Wording = RandomStringUtil.GenerateComplexMultiLineText(10_001));
    }

    private static UpdateInitiativeRequest NewValidRequest(Action<UpdateInitiativeRequest>? customizer = null)
    {
        var req = new UpdateInitiativeRequest
        {
            Id = "9f9a4845-a960-426f-9c12-6da6a0d6071b",
            SubTypeId = "0b05f532-b6b0-4e65-b195-bff4497f5ab4",
            Address = CollectionAddressTest.NewValidRequest(),
            Description = "foo bar",
            Wording = "foo bar",
        };
        customizer?.Invoke(req);
        return req;
    }
}
