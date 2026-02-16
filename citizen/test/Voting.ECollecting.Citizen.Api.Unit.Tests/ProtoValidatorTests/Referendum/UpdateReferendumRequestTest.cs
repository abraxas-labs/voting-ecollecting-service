// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class UpdateReferendumRequestTest : ProtoValidatorBaseTest<UpdateReferendumRequest>
{
    protected override IEnumerable<UpdateReferendumRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateSimpleSingleLineText(200));
        yield return NewValidRequest(x => x.Reason = string.Empty);
        yield return NewValidRequest(x => x.Reason = RandomStringUtil.GenerateComplexMultiLineText(1));
        yield return NewValidRequest(x => x.Reason = RandomStringUtil.GenerateComplexMultiLineText(10_000));
        yield return NewValidRequest(x => x.MembersCommittee = string.Empty);
        yield return NewValidRequest(x => x.MembersCommittee = RandomStringUtil.GenerateComplexMultiLineText(1));
        yield return NewValidRequest(x => x.MembersCommittee = RandomStringUtil.GenerateComplexMultiLineText(2_000));
        yield return NewValidRequest(x => x.Link = string.Empty);
        yield return NewValidRequest(x => x.Link = "https://example.com");
        yield return NewValidRequest(x => x.Link = RandomStringUtil.GenerateHttpsUrl(2_000));
    }

    protected override IEnumerable<UpdateReferendumRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = "invalid-guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Description = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateSimpleSingleLineText(201));
        yield return NewValidRequest(x => x.Description = "Te\nst");
        yield return NewValidRequest(x => x.Reason = RandomStringUtil.GenerateComplexMultiLineText(10_001));
        yield return NewValidRequest(x => x.MembersCommittee = RandomStringUtil.GenerateComplexMultiLineText(2_001));
        yield return NewValidRequest(x => x.Address = null);
        yield return NewValidRequest(x => x.Link = "http://example.com");
        yield return NewValidRequest(x => x.Link = "https://example\n.com");
        yield return NewValidRequest(x => x.Link = RandomStringUtil.GenerateHttpsUrl(2_001));
    }

    private UpdateReferendumRequest NewValidRequest(Action<UpdateReferendumRequest>? customizer = null)
    {
        var request = new UpdateReferendumRequest
        {
            Id = "26ed0f7c-ab7d-45e4-a3c3-d59d4af3a57a",
            Description = "Referendum gegen SBB Ausbau",
            Reason = "Mehr Geld für den Individualverkehr!",
            MembersCommittee = "Max Muster, Präsident TCS, St.Gallen",
            Address = CollectionAddressTest.NewValidRequest(),
            Link = "https://admin.ch",
        };

        customizer?.Invoke(request);
        return request;
    }
}
