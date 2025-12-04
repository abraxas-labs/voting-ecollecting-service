// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class CameAboutInitiativeRequestTest : ProtoValidatorBaseTest<CameAboutInitiativeRequest>
{
    protected override IEnumerable<CameAboutInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<CameAboutInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.SensitiveDataExpiryDate = null);
    }

    private static CameAboutInitiativeRequest NewValidRequest(Action<CameAboutInitiativeRequest>? customizer = null)
    {
        var request = new CameAboutInitiativeRequest
        {
            InitiativeId = "34db6a27-a543-44bd-8196-266927e3c360",
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
