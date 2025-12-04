// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteSignatureSheetRequestTest : ProtoValidatorBaseTest<DeleteSignatureSheetRequest>
{
    protected override IEnumerable<DeleteSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
    }

    private static DeleteSignatureSheetRequest NewValidRequest(Action<DeleteSignatureSheetRequest>? customizer = null)
    {
        var request = new DeleteSignatureSheetRequest
        {
            CollectionId = "e0d38afd-6abc-4f5c-9517-15571d8621c9",
            SignatureSheetId = "3498596c-35f9-4fc5-bfae-b86ac6044e3a",
        };

        customizer?.Invoke(request);
        return request;
    }
}
