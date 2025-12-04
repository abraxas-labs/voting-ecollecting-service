// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ConfirmSignatureSheetRequestTest : ProtoValidatorBaseTest<ConfirmSignatureSheetRequest>
{
    protected override IEnumerable<ConfirmSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.AddedPersonRegisterIds.Clear());
        yield return NewValidRequest(x => x.RemovedPersonRegisterIds.Clear());
    }

    protected override IEnumerable<ConfirmSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Unspecified);
        yield return NewValidRequest(x => x.CollectionType = (CollectionType)(-1));
        yield return NewValidRequest(x => x.AddedPersonRegisterIds.Add(string.Empty));
        yield return NewValidRequest(x => x.AddedPersonRegisterIds.Add("not a guid"));
        yield return NewValidRequest(x => x.RemovedPersonRegisterIds.Add(string.Empty));
        yield return NewValidRequest(x => x.RemovedPersonRegisterIds.Add("not a guid"));
        yield return NewValidRequest(x => x.SignatureCountTotal = -10);
        yield return NewValidRequest(x => x.SignatureCountTotal = 0);
    }

    private static ConfirmSignatureSheetRequest NewValidRequest(Action<ConfirmSignatureSheetRequest>? customizer = null)
    {
        var request = new ConfirmSignatureSheetRequest
        {
            CollectionId = "3b187424-41d4-49a1-86d3-e148d25e1c3e",
            SignatureSheetId = "89bb93ee-2be3-4410-8f8f-b665a19d1150",
            CollectionType = CollectionType.Initiative,
            AddedPersonRegisterIds = { "d9b90e3d-9d60-4549-b7f1-b893a5b9edac" },
            RemovedPersonRegisterIds = { "d9b90e3d-9d60-4549-b7f1-b893a5b9edac" },
            SignatureCountTotal = 10,
        };

        customizer?.Invoke(request);
        return request;
    }
}
