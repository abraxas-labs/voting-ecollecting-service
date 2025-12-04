// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class CollectionAddressTest : ProtoValidatorBaseTest<CollectionAddress>
{
    public static CollectionAddress NewValidRequest(Action<CollectionAddress>? customizer = null)
    {
        var request = new CollectionAddress
        {
            CommitteeOrPerson = "Verkehrsklub Schweiz",
            StreetOrPostOfficeBox = "Bundesstrasse 1",
            ZipCode = "3000",
            Locality = "Bern",
        };

        customizer?.Invoke(request);
        return request;
    }

    public static CollectionAddress NewInvalidRequest(Action<CollectionAddress>? customizer = null)
    {
        var request = NewValidRequest(x => x.CommitteeOrPerson = string.Empty);
        customizer?.Invoke(request);
        return request;
    }

    protected override IEnumerable<CollectionAddress> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.CommitteeOrPerson = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewValidRequest(x => x.CommitteeOrPerson = RandomStringUtil.GenerateSimpleSingleLineText(100));
        yield return NewValidRequest(x => x.StreetOrPostOfficeBox = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewValidRequest(x => x.StreetOrPostOfficeBox = RandomStringUtil.GenerateSimpleSingleLineText(150));
        yield return NewValidRequest(x => x.ZipCode = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewValidRequest(x => x.ZipCode = RandomStringUtil.GenerateSimpleSingleLineText(15));
        yield return NewValidRequest(x => x.Locality = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewValidRequest(x => x.Locality = RandomStringUtil.GenerateSimpleSingleLineText(150));
    }

    protected override IEnumerable<CollectionAddress> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CommitteeOrPerson = string.Empty);
        yield return NewValidRequest(x => x.CommitteeOrPerson = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return NewValidRequest(x => x.CommitteeOrPerson = "Te\nst");
        yield return NewValidRequest(x => x.StreetOrPostOfficeBox = string.Empty);
        yield return NewValidRequest(x => x.StreetOrPostOfficeBox = RandomStringUtil.GenerateSimpleSingleLineText(151));
        yield return NewValidRequest(x => x.StreetOrPostOfficeBox = "Te\nst");
        yield return NewValidRequest(x => x.ZipCode = string.Empty);
        yield return NewValidRequest(x => x.ZipCode = RandomStringUtil.GenerateSimpleSingleLineText(16));
        yield return NewValidRequest(x => x.ZipCode = "Te\nst");
        yield return NewValidRequest(x => x.Locality = string.Empty);
        yield return NewValidRequest(x => x.Locality = RandomStringUtil.GenerateSimpleSingleLineText(151));
        yield return NewValidRequest(x => x.Locality = "Te\nst");
    }
}
