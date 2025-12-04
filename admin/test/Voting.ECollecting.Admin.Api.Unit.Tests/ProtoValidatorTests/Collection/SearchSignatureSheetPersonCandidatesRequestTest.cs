// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class SearchSignatureSheetPersonCandidatesRequestTest : ProtoValidatorBaseTest<SearchSignatureSheetPersonCandidatesRequest>
{
    protected override IEnumerable<SearchSignatureSheetPersonCandidatesRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.DateOfBirth = null);
        yield return NewValidRequest(x => x.FirstName = string.Empty);
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexSingleLineText(2));
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexSingleLineText(100));
        yield return NewValidRequest(x => x.OfficialName = string.Empty);
        yield return NewValidRequest(x => x.OfficialName = RandomStringUtil.GenerateComplexSingleLineText(2));
        yield return NewValidRequest(x => x.OfficialName = RandomStringUtil.GenerateComplexSingleLineText(100));
        yield return NewValidRequest(x => x.ResidenceAddressStreet = string.Empty);
        yield return NewValidRequest(x => x.ResidenceAddressStreet = RandomStringUtil.GenerateComplexSingleLineText(2));
        yield return NewValidRequest(x => x.ResidenceAddressStreet = RandomStringUtil.GenerateComplexSingleLineText(150));
        yield return NewValidRequest(x => x.ResidenceAddressHouseNumber = string.Empty);
        yield return NewValidRequest(x => x.ResidenceAddressHouseNumber = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.ResidenceAddressHouseNumber = RandomStringUtil.GenerateComplexSingleLineText(150));
    }

    protected override IEnumerable<SearchSignatureSheetPersonCandidatesRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionType = (CollectionType)(-1));
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexSingleLineText(101));
        yield return NewValidRequest(x => x.OfficialName = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.OfficialName = RandomStringUtil.GenerateComplexSingleLineText(101));
        yield return NewValidRequest(x => x.ResidenceAddressStreet = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.ResidenceAddressStreet = RandomStringUtil.GenerateComplexSingleLineText(151));
        yield return NewValidRequest(x => x.ResidenceAddressHouseNumber = RandomStringUtil.GenerateComplexSingleLineText(151));
    }

    private static SearchSignatureSheetPersonCandidatesRequest NewValidRequest(Action<SearchSignatureSheetPersonCandidatesRequest>? customizer = null)
    {
        var request = new SearchSignatureSheetPersonCandidatesRequest
        {
            CollectionId = "d935e1c8-f629-4c3c-ae6c-0aad5084dad2",
            CollectionType = CollectionType.Initiative,
            SignatureSheetId = "efc594d7-b143-447a-94cf-2679a682ee2a",
            DateOfBirth = MockedClock.GetTimestamp(12),
            FirstName = "foo",
            OfficialName = "bar",
            ResidenceAddressHouseNumber = "12",
            ResidenceAddressStreet = "foo street",
        };

        customizer?.Invoke(request);
        return request;
    }
}
