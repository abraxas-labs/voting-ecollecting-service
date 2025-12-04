// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class SetSensitiveDataExpiryDateRequestTest : ProtoValidatorBaseTest<SetInitiativeSensitiveDataExpiryDateRequest>
{
    protected override IEnumerable<SetInitiativeSensitiveDataExpiryDateRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SetInitiativeSensitiveDataExpiryDateRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.SensitiveDataExpiryDate = null);
    }

    private static SetInitiativeSensitiveDataExpiryDateRequest NewValidRequest(Action<SetInitiativeSensitiveDataExpiryDateRequest>? customizer = null)
    {
        var request = new SetInitiativeSensitiveDataExpiryDateRequest
        {
            InitiativeId = "54345774-02dc-4aa6-8aac-48ff177bbbf9",
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
