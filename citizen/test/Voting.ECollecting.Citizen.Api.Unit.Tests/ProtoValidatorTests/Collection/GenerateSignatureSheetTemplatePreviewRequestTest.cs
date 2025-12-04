// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class GenerateSignatureSheetTemplatePreviewRequestTest : ProtoValidatorBaseTest<GenerateSignatureSheetTemplatePreviewRequest>
{
    protected override IEnumerable<GenerateSignatureSheetTemplatePreviewRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Referendum);
    }

    protected override IEnumerable<GenerateSignatureSheetTemplatePreviewRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Unspecified);
        yield return NewValidRequest(x => x.CollectionType = (CollectionType)(-1));
    }

    private static GenerateSignatureSheetTemplatePreviewRequest NewValidRequest(Action<GenerateSignatureSheetTemplatePreviewRequest>? customizer = null)
    {
        var request = new GenerateSignatureSheetTemplatePreviewRequest
        {
            Id = "cd7fc122-10dd-4db1-a108-f4f67ca4c0fd",
            CollectionType = CollectionType.Initiative,
        };

        customizer?.Invoke(request);
        return request;
    }
}
