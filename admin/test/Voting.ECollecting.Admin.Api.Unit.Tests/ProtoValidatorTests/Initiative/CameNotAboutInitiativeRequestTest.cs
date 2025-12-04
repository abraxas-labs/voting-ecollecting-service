// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class CameNotAboutInitiativeRequestTest : ProtoValidatorBaseTest<CameNotAboutInitiativeRequest>
{
    protected override IEnumerable<CameNotAboutInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<CameNotAboutInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.Reason = CollectionCameNotAboutReason.Unspecified);
        yield return NewValidRequest(x => x.Reason = (CollectionCameNotAboutReason)(-1));
        yield return NewValidRequest(x => x.SensitiveDataExpiryDate = null);
    }

    private static CameNotAboutInitiativeRequest NewValidRequest(Action<CameNotAboutInitiativeRequest>? customizer = null)
    {
        var request = new CameNotAboutInitiativeRequest
        {
            InitiativeId = "7a65608a-a296-4b45-8bc7-3f52257ce03e",
            Reason = CollectionCameNotAboutReason.MinSignatureCountNotReached,
            SensitiveDataExpiryDate = new Date
            {
                Day = 10,
                Month = 12,
                Year = 2020,
            },
        };

        customizer?.Invoke(request);
        return request;
    }
}
