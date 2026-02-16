// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class AddSignatureSheetRequestTest : ProtoValidatorBaseTest<AddSignatureSheetRequest>
{
    protected override IEnumerable<AddSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<AddSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.Number = 0);
        yield return NewValidRequest(x => x.Number = -1);
        yield return NewValidRequest(x => x.SignatureCountTotal = 0);
        yield return NewValidRequest(x => x.SignatureCountTotal = -1);
        yield return NewValidRequest(x => x.ReceivedAt = null);
    }

    private static AddSignatureSheetRequest NewValidRequest(Action<AddSignatureSheetRequest>? customizer = null)
    {
        var request = new AddSignatureSheetRequest
        {
            CollectionId = "96095807-d16d-4eca-a4f6-e92ecb1b31d5",
            Number = 10,
            ReceivedAt = new Date { Day = 10, Month = 12, Year = 2020, },
            SignatureCountTotal = 100,
        };

        customizer?.Invoke(request);
        return request;
    }
}
