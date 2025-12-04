// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Google.Protobuf.WellKnownTypes;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class UpdateSignatureSheetRequestTest : ProtoValidatorBaseTest<UpdateSignatureSheetRequest>
{
    protected override IEnumerable<UpdateSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<UpdateSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.SignatureCountTotal = 0);
        yield return NewValidRequest(x => x.SignatureCountTotal = -1);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.ReceivedAt = null);
    }

    private static UpdateSignatureSheetRequest NewValidRequest(Action<UpdateSignatureSheetRequest>? customizer = null)
    {
        var request = new UpdateSignatureSheetRequest
        {
            ReceivedAt = Timestamp.FromDateTime(MockedClock.UtcNowDate),
            CollectionId = "613a50a4-2494-45b8-b656-59b4b7d9ecf0",
            SignatureSheetId = "a243a0f8-1275-4b83-91b1-b59064ffbb97",
            SignatureCountTotal = 1_000,
        };

        customizer?.Invoke(request);
        return request;
    }
}
