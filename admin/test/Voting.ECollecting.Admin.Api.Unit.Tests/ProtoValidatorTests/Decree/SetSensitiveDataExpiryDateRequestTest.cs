// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Decree;

public class SetSensitiveDataExpiryDateRequestTest : ProtoValidatorBaseTest<SetDecreeSensitiveDataExpiryDateRequest>
{
    protected override IEnumerable<SetDecreeSensitiveDataExpiryDateRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SetDecreeSensitiveDataExpiryDateRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.DecreeId = string.Empty);
        yield return NewValidRequest(x => x.DecreeId = "not a guid");
        yield return NewValidRequest(x => x.SensitiveDataExpiryDate = null);
    }

    private static SetDecreeSensitiveDataExpiryDateRequest NewValidRequest(Action<SetDecreeSensitiveDataExpiryDateRequest>? customizer = null)
    {
        var request = new SetDecreeSensitiveDataExpiryDateRequest
        {
            DecreeId = "54345774-02dc-4aa6-8aac-48ff177bbbf9",
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
