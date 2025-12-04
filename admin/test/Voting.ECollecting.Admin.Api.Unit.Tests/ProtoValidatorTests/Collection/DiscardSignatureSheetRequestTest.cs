// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DiscardSignatureSheetRequestTest : ProtoValidatorBaseTest<DiscardSignatureSheetRequest>
{
    protected override IEnumerable<DiscardSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DiscardSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
    }

    private static DiscardSignatureSheetRequest NewValidRequest(Action<DiscardSignatureSheetRequest>? customizer = null)
    {
        var request = new DiscardSignatureSheetRequest
        {
            CollectionId = "b1330356-49a6-4516-99d6-ffa68b838ec0",
            SignatureSheetId = "04f35021-5ae5-4494-b129-6ffa6eaa7004",
        };

        customizer?.Invoke(request);
        return request;
    }
}
