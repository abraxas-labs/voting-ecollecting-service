// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteSignatureSheetTemplateRequestTest : ProtoValidatorBaseTest<DeleteSignatureSheetTemplateRequest>
{
    protected override IEnumerable<DeleteSignatureSheetTemplateRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteSignatureSheetTemplateRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static DeleteSignatureSheetTemplateRequest NewValidRequest(Action<DeleteSignatureSheetTemplateRequest>? customizer = null)
    {
        var request = new DeleteSignatureSheetTemplateRequest
        {
            Id = "533b4054-90a5-4a45-be62-8429840ee898",
        };

        customizer?.Invoke(request);
        return request;
    }
}
