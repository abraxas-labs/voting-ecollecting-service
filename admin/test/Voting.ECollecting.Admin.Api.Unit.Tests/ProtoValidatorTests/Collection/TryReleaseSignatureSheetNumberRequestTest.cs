// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class TryReleaseSignatureSheetNumberRequestTest : ProtoValidatorBaseTest<TryReleaseSignatureSheetNumberRequest>
{
    protected override IEnumerable<TryReleaseSignatureSheetNumberRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<TryReleaseSignatureSheetNumberRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.Number = 0);
        yield return NewValidRequest(x => x.Number = -1);
    }

    private static TryReleaseSignatureSheetNumberRequest NewValidRequest(Action<TryReleaseSignatureSheetNumberRequest>? customizer = null)
    {
        var request = new TryReleaseSignatureSheetNumberRequest
        {
            CollectionId = "96095807-d16d-4eca-a4f6-e92ecb1b31d5",
            Number = 10,
        };

        customizer?.Invoke(request);
        return request;
    }
}
