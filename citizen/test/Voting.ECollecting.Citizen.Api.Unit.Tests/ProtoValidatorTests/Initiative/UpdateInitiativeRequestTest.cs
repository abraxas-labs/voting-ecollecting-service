// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class UpdateInitiativeRequestTest : ProtoValidatorBaseTest<UpdateInitiativeRequest>
{
    protected override IEnumerable<UpdateInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.SubTypeId = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(200));
        yield return NewValidRequest(x => x.Wording = RandomStringUtil.GenerateComplexMultiLineText(1));
        yield return NewValidRequest(x => x.Wording = RandomStringUtil.GenerateComplexMultiLineText(10_000));
        yield return NewValidRequest(x => x.Reason = RandomStringUtil.GenerateComplexMultiLineText(1));
        yield return NewValidRequest(x => x.Reason = RandomStringUtil.GenerateComplexMultiLineText(10_000));
        yield return NewValidRequest(x => x.Reason = string.Empty);
        yield return NewValidRequest(x => x.Link = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.Link = RandomStringUtil.GenerateComplexSingleLineText(2_000));
        yield return NewValidRequest(x => x.Link = string.Empty);
    }

    protected override IEnumerable<UpdateInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
        yield return NewValidRequest(x => x.SubTypeId = "not a guid");
        yield return NewValidRequest(x => x.Description = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(201));
        yield return NewValidRequest(x => x.Description = "Te\nst");
        yield return NewValidRequest(x => x.Wording = string.Empty);
        yield return NewValidRequest(x => x.Wording = RandomStringUtil.GenerateComplexMultiLineText(10_001));
        yield return NewValidRequest(x => x.Reason = RandomStringUtil.GenerateComplexMultiLineText(10_001));
        yield return NewValidRequest(x => x.Address = null);
        yield return NewValidRequest(x => x.Link = RandomStringUtil.GenerateComplexSingleLineText(2_001));
        yield return NewValidRequest(x => x.Link = "Te\nst");
    }

    private static UpdateInitiativeRequest NewValidRequest(Action<UpdateInitiativeRequest>? customizer = null)
    {
        var request = new UpdateInitiativeRequest
        {
            Id = "cf1377d0-447e-4cd6-8dc4-5545dd3b043c",
            SubTypeId = "0061b912-8797-4928-ab11-3b1b70baad95",
            Description = "Initiative",
            Wording = "Wortlaut",
            Reason = "Begründung",
            Address = CollectionAddressTest.NewValidRequest(),
            Link = "https://www.example.com",
        };

        customizer?.Invoke(request);
        return request;
    }
}
