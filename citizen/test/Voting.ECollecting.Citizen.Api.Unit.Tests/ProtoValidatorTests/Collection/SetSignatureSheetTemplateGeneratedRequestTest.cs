// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class SetSignatureSheetTemplateGeneratedRequestTest : ProtoValidatorBaseTest<SetSignatureSheetTemplateGeneratedRequest>
{
    protected override IEnumerable<SetSignatureSheetTemplateGeneratedRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Referendum);
    }

    protected override IEnumerable<SetSignatureSheetTemplateGeneratedRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Unspecified);
        yield return NewValidRequest(x => x.CollectionType = (CollectionType)(-1));
    }

    private static SetSignatureSheetTemplateGeneratedRequest NewValidRequest(Action<SetSignatureSheetTemplateGeneratedRequest>? customizer = null)
    {
        var request = new SetSignatureSheetTemplateGeneratedRequest
        {
            Id = "20651a6b-a584-4047-b873-baf2b24ee9a1",
            CollectionType = CollectionType.Initiative,
        };

        customizer?.Invoke(request);
        return request;
    }
}
