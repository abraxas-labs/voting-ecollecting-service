// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ListSignatureSheetCitizensRequestTest : ProtoValidatorBaseTest<ListSignatureSheetCitizensRequest>
{
    protected override IEnumerable<ListSignatureSheetCitizensRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ListSignatureSheetCitizensRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "invalid-guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "invalid-guid");
    }

    private static ListSignatureSheetCitizensRequest NewValidRequest(
        Action<ListSignatureSheetCitizensRequest>? customizer = null)
    {
        var request = new ListSignatureSheetCitizensRequest
        {
            CollectionId = "6d452445-97a4-497e-bef2-09a48c95304a",
            SignatureSheetId = "39c224b7-bfff-48ee-9727-2f8ae461447f",
        };
        customizer?.Invoke(request);
        return request;
    }
}
