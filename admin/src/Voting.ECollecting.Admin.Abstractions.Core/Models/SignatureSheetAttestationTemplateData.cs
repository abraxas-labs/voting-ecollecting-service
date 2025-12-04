// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Models;

public record SignatureSheetAttestationTemplateData(
    CollectionBaseEntity Collection,
    AccessControlListDoiEntity AclDomainOfInfluence,
    DomainOfInfluenceEntity DomainOfInfluence,
    int SignatureListCount,
    IEnumerable<SignatureSheetListDataContainer> SignatureList,
    int ValidSignatureCount,
    int InvalidSignatureCount,
    DateTime CertificationDate,
    DateTime CollectionStartDate,
    string CollectionType,
    string ReferendumNumber);
