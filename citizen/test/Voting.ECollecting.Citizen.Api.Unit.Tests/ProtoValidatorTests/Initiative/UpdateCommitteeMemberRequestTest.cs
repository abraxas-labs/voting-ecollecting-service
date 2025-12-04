// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class UpdateCommitteeMemberRequestTest : ProtoValidatorBaseTest<UpdateCommitteeMemberRequest>
{
    protected override IEnumerable<UpdateCommitteeMemberRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Email = string.Empty);
        yield return NewValidRequest(x => x.PoliticalDuty = string.Empty);
        yield return NewValidRequest(x => x.Role = CollectionPermissionRole.Unspecified);
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexSingleLineText(100));
        yield return NewValidRequest(x => x.LastName = RandomStringUtil.GenerateComplexSingleLineText(100));
        yield return NewValidRequest(x => x.PoliticalFirstName = RandomStringUtil.GenerateComplexSingleLineText(100));
        yield return NewValidRequest(x => x.PoliticalLastName = RandomStringUtil.GenerateComplexSingleLineText(100));
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(8));
        yield return NewValidRequest(x => x.PoliticalBfs = RandomStringUtil.GenerateAlphanumericWhitespace(8));
    }

    protected override IEnumerable<UpdateCommitteeMemberRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = "invalid-guid");
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.Id = "invalid-guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.FirstName = string.Empty);
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexMultiLineText(200));
        yield return NewValidRequest(x => x.LastName = string.Empty);
        yield return NewValidRequest(x => x.LastName = RandomStringUtil.GenerateComplexMultiLineText(200));
        yield return NewValidRequest(x => x.PoliticalFirstName = string.Empty);
        yield return NewValidRequest(x => x.PoliticalFirstName = RandomStringUtil.GenerateComplexMultiLineText(200));
        yield return NewValidRequest(x => x.PoliticalLastName = string.Empty);
        yield return NewValidRequest(x => x.PoliticalLastName = RandomStringUtil.GenerateComplexMultiLineText(200));
        yield return NewValidRequest(x => x.DateOfBirth = null);
        yield return NewValidRequest(x => x.Bfs = string.Empty);
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(9));
        yield return NewValidRequest(x => x.PoliticalBfs = string.Empty);
        yield return NewValidRequest(x => x.PoliticalBfs = RandomStringUtil.GenerateAlphanumericWhitespace(9));
        yield return NewValidRequest(x => x.Email = "foo");
    }

    private UpdateCommitteeMemberRequest NewValidRequest(Action<UpdateCommitteeMemberRequest>? customizer = null)
    {
        var request = new UpdateCommitteeMemberRequest
        {
            Id = "74d415cd-916c-4571-9c73-517b94b56cec",
            InitiativeId = "751b2313-e7d3-4b98-bc7e-56097d8b4e9a",
            Email = "foo@example.com",
            FirstName = "Foo",
            LastName = "Bar",
            PoliticalFirstName = "Foo (pol)",
            PoliticalLastName = "Bar (pol)",
            DateOfBirth = MockedClock.GetTimestamp(-55 * 365),
            RequestMemberSignature = true,
            PoliticalDuty = "Protokollführer",
            PoliticalBfs = "3203",
            Bfs = "3203",
            Role = CollectionPermissionRole.Deputy,
        };

        customizer?.Invoke(request);
        return request;
    }
}
