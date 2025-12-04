// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class CreateInitiativeRequestTest : ProtoValidatorBaseTest<CreateInitiativeRequest>
{
    protected override IEnumerable<CreateInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(1));
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(8));
        yield return NewValidRequest(x => x.Bfs = string.Empty);
        yield return NewValidRequest(x => x.SubTypeId = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(200));
    }

    protected override IEnumerable<CreateInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.DomainOfInfluenceType = DomainOfInfluenceType.Unspecified);
        yield return NewValidRequest(x => x.DomainOfInfluenceType = (DomainOfInfluenceType)(-1));
        yield return NewValidRequest(x => x.Bfs = "3203-12");
        yield return NewValidRequest(x => x.SubTypeId = "not a guid");
        yield return NewValidRequest(x => x.Description = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(201));
        yield return NewValidRequest(x => x.Description = "Te\nst");
    }

    private static CreateInitiativeRequest NewValidRequest(Action<CreateInitiativeRequest>? customizer = null)
    {
        var request = new CreateInitiativeRequest
        {
            DomainOfInfluenceType = DomainOfInfluenceType.Ct,
            Bfs = "3203",
            SubTypeId = "d2ca2f9b-c202-4bf6-aff2-bca59852f5a4",
            Description = "Initiative",
        };

        customizer?.Invoke(request);
        return request;
    }
}
