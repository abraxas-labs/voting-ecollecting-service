// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Core.Models;

namespace Voting.ECollecting.Admin.Core.Services.Documents.TemplateBag;

public record SignatureSheetAttestationTemplateBag(
    string? MunicipalityLogo,
    string MunicipalityAddressName,
    string MunicipalityStreet,
    string MunicipalityZipCode,
    string MunicipalityLocality,
    string? MunicipalityPhoneNumber,
    string? MunicipalityEmail,
    string? MunicipalityWebsite,
    string CommitteeName,
    string CommitteeStreet,
    string CommitteeZipCode,
    string CommitteeLocality,
    string DomainOfInfluenceType,
    string CollectionType,
    string CollectionDescription,
    string CollectionStartDate,
    string MunicipalityName,
    string MunicipalityNameIncludingType,
    int SignatureListCount,
    IEnumerable<SignatureSheetListDataContainer> SignatureList,
    int ValidSignatureCount,
    int InvalidSignatureCount,
    string CertificationDate,
    string ReferendumNumber
);
