// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Models;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Accessibility;

public class SendAccessibilityMessageRequestTest : ProtoValidatorBaseTest<SendAccessibilityMessageRequest>
{
    protected override IEnumerable<SendAccessibilityMessageRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Salutation = AccessibilitySalutation.Unspecified);
        yield return NewValidRequest(x => x.FirstName = string.Empty);
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateSimpleSingleLineText(100));
        yield return NewValidRequest(x => x.LastName = string.Empty);
        yield return NewValidRequest(x => x.LastName = RandomStringUtil.GenerateSimpleSingleLineText(100));
        yield return NewValidRequest(x => x.Phone = string.Empty);
        yield return NewValidRequest(x => x.Message = RandomStringUtil.GenerateComplexMultiLineText(1_000));
    }

    protected override IEnumerable<SendAccessibilityMessageRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Salutation = (AccessibilitySalutation)(-1));
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return NewValidRequest(x => x.LastName = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return NewValidRequest(x => x.Email = string.Empty);
        yield return NewValidRequest(x => x.Email = "not a email");
        yield return NewValidRequest(x => x.Phone = "not a phone");
        yield return NewValidRequest(x => x.Category = AccessibilityCategory.Unspecified);
        yield return NewValidRequest(x => x.Category = (AccessibilityCategory)(-1));
        yield return NewValidRequest(x => x.Message = string.Empty);
        yield return NewValidRequest(x => x.Message = RandomStringUtil.GenerateComplexMultiLineText(1_001));
    }

    private SendAccessibilityMessageRequest NewValidRequest(Action<SendAccessibilityMessageRequest>? modifier = null)
    {
        var req = new SendAccessibilityMessageRequest
        {
            Salutation = AccessibilitySalutation.Mrs,
            FirstName = "Petra",
            LastName = "Muster",
            Email = "petra.muster@test.com",
            Phone = "+41 79 456 79 90",
            Category = AccessibilityCategory.Various,
            Message = "Test Message",
        };
        modifier?.Invoke(req);
        return req;
    }
}
