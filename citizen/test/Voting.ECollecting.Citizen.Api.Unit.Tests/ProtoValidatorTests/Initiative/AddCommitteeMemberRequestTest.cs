// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class AddCommitteeMemberRequestTest : ProtoValidatorBaseTest<AddCommitteeMemberRequest>
{
    protected override IEnumerable<AddCommitteeMemberRequest> OkMessages()
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

    protected override IEnumerable<AddCommitteeMemberRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = "invalid-guid");
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
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

    private AddCommitteeMemberRequest NewValidRequest(Action<AddCommitteeMemberRequest>? customizer = null)
    {
        var request = new AddCommitteeMemberRequest
        {
            Email = "foo@example.com",
            FirstName = "Foo",
            LastName = "Bar",
            PoliticalFirstName = "Foo (pol)",
            PoliticalLastName = "Bar (pol)",
            DateOfBirth = MockedClock.GetTimestamp(-55 * 365),
            InitiativeId = "eb62e2f0-1f88-4d5b-bbe9-0ab0878b63ec",
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
