// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Globalization;
using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TemplateBagMapper
{
    [MapNestedProperties(nameof(InitiativeTemplateData.Initiative))]
    [MapProperty([nameof(InitiativeTemplateData.Initiative), nameof(InitiativeTemplateData.Initiative.Address), nameof(InitiativeTemplateData.Initiative.Address.CommitteeOrPerson)], nameof(InitiativeSignatureSheetTemplateBag.CommitteeName))]
    [MapProperty([nameof(InitiativeTemplateData.Initiative), nameof(InitiativeTemplateData.Initiative.Address), nameof(InitiativeTemplateData.Initiative.Address.StreetOrPostOfficeBox)], nameof(InitiativeSignatureSheetTemplateBag.CommitteeStreet))]
    [MapProperty([nameof(InitiativeTemplateData.Initiative), nameof(InitiativeTemplateData.Initiative.Address), nameof(InitiativeTemplateData.Initiative.Address.ZipCode)], nameof(InitiativeSignatureSheetTemplateBag.CommitteeZipCode))]
    [MapProperty([nameof(InitiativeTemplateData.Initiative), nameof(InitiativeTemplateData.Initiative.Address), nameof(InitiativeTemplateData.Initiative.Address.Locality)], nameof(InitiativeSignatureSheetTemplateBag.CommitteeLocality))]
    [MapProperty(nameof(InitiativeTemplateData.Initiative.Link), nameof(InitiativeSignatureSheetTemplateBag.Website))]
    [MapProperty([nameof(InitiativeTemplateData.Initiative), nameof(InitiativeTemplateData.Initiative.Image), nameof(InitiativeTemplateData.Initiative.Image.Content)], nameof(InitiativeSignatureSheetTemplateBag.Image))]
    [MapProperty([nameof(InitiativeTemplateData.Initiative), nameof(InitiativeTemplateData.Initiative.Logo), nameof(InitiativeTemplateData.Initiative.Logo.Content)], nameof(InitiativeSignatureSheetTemplateBag.Logo))]
    [MapProperty(nameof(InitiativeTemplateData.Initiative.SignatureListSubmissionEndDate), nameof(InitiativeSignatureSheetTemplateBag.SignatureListSubmissionEndDate))]
    [MapProperty(nameof(InitiativeTemplateData.CommitteeMembers), nameof(InitiativeSignatureSheetTemplateBag.CommitteeMembers))]
    public static partial InitiativeSignatureSheetTemplateBag MapToInitiativeSignatureSheetTemplateBag(InitiativeTemplateData initiativeTemplate);

    [MapProperty(nameof(ReferendumEntity.Decree.Description), nameof(ReferendumSignatureSheetTemplateBag.DecreeDescription))]
    [MapProperty(nameof(ReferendumEntity.MembersCommittee), nameof(ReferendumSignatureSheetTemplateBag.CommitteeMembers))]
    [MapProperty(nameof(ReferendumEntity.Address.CommitteeOrPerson), nameof(ReferendumSignatureSheetTemplateBag.CommitteeName))]
    [MapProperty(nameof(ReferendumEntity.Address.StreetOrPostOfficeBox), nameof(ReferendumSignatureSheetTemplateBag.CommitteeStreet))]
    [MapProperty(nameof(ReferendumEntity.Address.ZipCode), nameof(ReferendumSignatureSheetTemplateBag.CommitteeZipCode))]
    [MapProperty(nameof(ReferendumEntity.Address.Locality), nameof(ReferendumSignatureSheetTemplateBag.CommitteeLocality))]
    [MapProperty(nameof(ReferendumEntity.Link), nameof(ReferendumSignatureSheetTemplateBag.Website))]
    [MapProperty(nameof(ReferendumEntity.Image.Content), nameof(ReferendumSignatureSheetTemplateBag.Image))]
    [MapProperty(nameof(ReferendumEntity.Logo.Content), nameof(ReferendumSignatureSheetTemplateBag.Logo))]
    [MapProperty(nameof(ReferendumEntity.SignatureListSubmissionEndDate), nameof(ReferendumSignatureSheetTemplateBag.SignatureListSubmissionEndDate))]
    public static partial ReferendumSignatureSheetTemplateBag MapToReferendumSignatureSheetTemplateBag(ReferendumEntity collection);

    [MapProperty(nameof(InitiativeCommitteeMember.DateOfBirth.Day), nameof(InitiativeCommitteeMemberDataContainer.DayOfBirth))]
    [MapProperty(nameof(InitiativeCommitteeMember.DateOfBirth.Month), nameof(InitiativeCommitteeMemberDataContainer.MonthOfBirth))]
    [MapProperty(nameof(InitiativeCommitteeMember.DateOfBirth.Year), nameof(InitiativeCommitteeMemberDataContainer.YearOfBirth))]
    public static partial InitiativeCommitteeMemberDataContainer MapToCommitteeMember(InitiativeCommitteeMember committeeMember);

    public static partial List<InitiativeCommitteeMemberDataContainer> MapToCommitteeMembers(IEnumerable<InitiativeCommitteeMember> committeeMembers);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(DomainOfInfluenceDataContainer.DomainOfInfluences))]
    [MapProperty(nameof(CollectionMunicipalityEntity.MunicipalityName), nameof(DomainOfInfluenceDataContainer.Name))]
    [MapProperty(nameof(CollectionMunicipalityEntity.ElectronicCitizenCount), nameof(DomainOfInfluenceDataContainer.ValidElectronicSignatureCount))]
    [MapProperty([nameof(CollectionMunicipalityEntity.PhysicalCount), nameof(CollectionMunicipalityEntity.PhysicalCount.Valid)], nameof(DomainOfInfluenceDataContainer.ValidPhysicalSignatureCount))]
    [MapProperty([nameof(CollectionMunicipalityEntity.PhysicalCount), nameof(CollectionMunicipalityEntity.PhysicalCount.Invalid)], nameof(DomainOfInfluenceDataContainer.InvalidPhysicalSignatureCount))]
    public static partial DomainOfInfluenceDataContainer MapToDomainOfInfluenceDataContainer(CollectionMunicipalityEntity municipality);

    public static partial string MapToString(DomainOfInfluenceType domainOfInfluenceType);

    public static partial List<CollectionPermissionDataContainer> MapToCollectionPermissionDataContainers(IEnumerable<CollectionPermissionEntity> collectionPermissions);

    private static partial CollectionPermissionDataContainer MapToCollectionPermissionDataContainer(CollectionPermissionEntity collectionPermission);

    private static string? MapByteArrayToBase64String(FileContentEntity? file)
    {
        return file == null ? null : Convert.ToBase64String(file.Data);
    }

    private static string MapDate(DateTime? date)
    {
        return date?.ToString("o", CultureInfo.InvariantCulture) ?? DateTime.MinValue.ToString("o", CultureInfo.InvariantCulture);
    }
}
