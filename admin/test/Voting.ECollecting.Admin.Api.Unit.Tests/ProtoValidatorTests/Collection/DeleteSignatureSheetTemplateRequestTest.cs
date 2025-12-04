// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteSignatureSheetTemplateRequestTest : ProtoValidatorBaseTest<DeleteSignatureSheetTemplateRequest>
{
    protected override IEnumerable<DeleteSignatureSheetTemplateRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Referendum);
    }

    protected override IEnumerable<DeleteSignatureSheetTemplateRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Unspecified);
        yield return NewValidRequest(x => x.CollectionType = (CollectionType)(-1));
    }

    private static DeleteSignatureSheetTemplateRequest NewValidRequest(Action<DeleteSignatureSheetTemplateRequest>? customizer = null)
    {
        var request = new DeleteSignatureSheetTemplateRequest
        {
            CollectionId = "e0d38afd-6abc-4f5c-9517-15571d8621c9",
            CollectionType = CollectionType.Initiative,
        };

        customizer?.Invoke(request);
        return request;
    }
}
