// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class RestoreSignatureSheetRequestTest : ProtoValidatorBaseTest<RestoreSignatureSheetRequest>
{
    protected override IEnumerable<RestoreSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<RestoreSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
    }

    private static RestoreSignatureSheetRequest NewValidRequest(Action<RestoreSignatureSheetRequest>? customizer = null)
    {
        var request = new RestoreSignatureSheetRequest
        {
            CollectionId = "3b187424-41d4-49a1-86d3-e148d25e1c3e",
            SignatureSheetId = "89bb93ee-2be3-4410-8f8f-b665a19d1150",
        };

        customizer?.Invoke(request);
        return request;
    }
}
