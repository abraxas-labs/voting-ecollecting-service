// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Enums;
using Google.Protobuf.WellKnownTypes;
using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Proto.Admin.Services.V1.Enums;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Admin.Services.V1.Responses;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Models;
using DomainEnums = Voting.ECollecting.Shared.Domain.Enums;
using DomainModels = Voting.ECollecting.Admin.Domain.Models;
using Pageable = Voting.Lib.Database.Models.Pageable;
using PageInfo = Voting.Lib.Database.Models.PageInfo;
using ProtoAdminModels = Voting.ECollecting.Proto.Admin.Services.V1.Models;
using SharedDomainModels = Voting.ECollecting.Shared.Domain.Models;
using SharedProtoModels = Abraxas.Voting.Ecollecting.Shared.V1.Models;

namespace Voting.ECollecting.Admin.Api.Grpc.Mappings;

[Mapper]
internal static partial class Mapper
{
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DomainModels.UpdateDomainOfInfluenceRequest MapToDomainOfInfluenceUpdate(UpdateDomainOfInfluenceRequest request);

    [MapEnum(EnumMappingStrategy.ByName)]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DomainModels.CollectionControlSignFilter MapToCollectionControlSignFilter(
        CollectionControlSignFilter filter);

    internal static ListCollectionsForDeletionResponse MapToListCollectionsForDeletionResponse(
        IReadOnlyDictionary<DomainEnums.DomainOfInfluenceType, DomainModels.CollectionsGroup> groups)
        => new ListCollectionsForDeletionResponse { Groups = { MapToCollectionsGroup(groups) } };

    internal static partial ProtoAdminModels.SecondFactorTransaction MapSecondFactorTransaction(
        DomainModels.SecondFactorTransactionInfo transaction);

    internal static partial DomainModels.CreateInitiativeParams MapToCreateInitiativeParams(
        CreateInitiativeWithAdmissibilityDecisionRequest request);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DomainModels.UpdateReferendumParams MapToUpdateReferendumParams(
        UpdateReferendumRequest request);

    internal static partial IReadOnlySet<DateTime> MapTimestampsToSet(IEnumerable<Timestamp> data);

    internal static partial DomainEnums.SortDirection MapToSortDirection(SortDirection dir);

    internal static partial Pageable? MapToPageable(SharedProtoModels.Pageable? pageable);

    internal static ListEligibleForAdmissibilityDecisionResponse MapToListEligibleForAdmissibilityDecisionResponse(
        IEnumerable<DomainModels.Initiative> initiatives)
        => new ListEligibleForAdmissibilityDecisionResponse { Initiatives = { MapToInitiatives(initiatives) } };

    internal static ListAdmissibilityDecisionsResponse MapToListAdmissibilityDecisionsResponse(
        IEnumerable<DomainModels.Initiative> initiatives)
        => new ListAdmissibilityDecisionsResponse { Initiatives = { MapToInitiatives(initiatives) } };

    internal static ListSignatureSheetCitizensResponse MapToListSignatureSheetCitizensResponse(IEnumerable<DomainModels.IVotingStimmregisterPersonInfo> citizens)
        => new ListSignatureSheetCitizensResponse { Citizens = { citizens.Select(MapPerson) } };

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial ReserveSignatureSheetNumberResponse MapToReserveSignatureSheetNumberResponse(
        DomainModels.CollectionSignatureSheetNumberInfo numberInfo);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DomainModels.ActiveCertificate.Certificate), nameof(GetActiveCertificateResponse.ActiveCertificate))]
    [MapProperty(nameof(DomainModels.ActiveCertificate.CACertificate), nameof(GetActiveCertificateResponse.CaCertificate))]
    internal static partial GetActiveCertificateResponse MapToActiveCertificateResponse(DomainModels.ActiveCertificate cert);

    internal static ListCertificatesResponse MapToListCertificatesResponse(IEnumerable<CertificateEntity> certs)
        => new() { Certificates = { MapToCertificates(certs) } };

    internal static partial DomainEnums.CollectionState MapToCollectionState(CollectionState state);

    internal static partial AdmissibilityDecisionState MapToAdmissibilityDecisionState(
        ProtoAdminModels.AdmissibilityDecisionState state);

    internal static partial InitiativeLockedFields? MapToInitiativeLockedFields(ProtoAdminModels.InitiativeLockedFields? lockedFields);

    internal static partial IReadOnlyCollection<DomainEnums.CollectionSignatureSheetState> MapToCollectionSignatureSheetStates(IEnumerable<ProtoAdminModels.CollectionSignatureSheetState> states);

    internal static partial DomainEnums.CollectionSignatureSheetSort MapToCollectionSignatureSheetSort(ListSignatureSheetsSort sort);

    internal static partial DomainEnums.CollectionType MapCollectionType(CollectionType collectionType);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DomainModels.UpdateInitiativeParams MapToUpdateInitiativeParams(UpdateInitiativeRequest request);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DomainModels.UpdateCommitteeMemberParams MapToUpdateCommitteeMemberParams(UpdateCommitteeMemberRequest request);

    internal static ListSignatureSheetsAttestedAtResponse MapToListSignatureSheetsAttestedAtResponse(
        IEnumerable<DateTime> src)
        => new ListSignatureSheetsAttestedAtResponse { AttestedAts = { MapToTimestamps(src) } };

    internal static ListSignatureSheetsResponse MapToListCollectionSignatureSheetsResponse(Page<DomainModels.CollectionSignatureSheet> sheets)
        => new()
        {
            PageInfo = MapToPageInfo(sheets),
            SignatureSheets = { MapToCollectionSignatureSheets(sheets.Items) },
        };

    internal static partial IEnumerable<ProtoAdminModels.CollectionSignatureSheet> MapToCollectionSignatureSheets(IEnumerable<DomainModels.CollectionSignatureSheet> sheets);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial DomainModels.Decree MapToDecree(CreateDecreeRequest request);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void MapToDecree(UpdateDecreeRequest request, DomainModels.Decree decree);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial CreateDecreeResponse MapToCreateDecreeResponse(DomainModels.Decree decree);

    internal static ListDecreesResponse MapToListDecreesResponse(IEnumerable<DomainModels.Decree> decrees)
        => new ListDecreesResponse { Decrees = { MapToDecrees(decrees) } };

    internal static partial GetDecreeForDeleteResponse MapToGetDecreeForDeleteResponse(DomainModels.DeleteDecreeInfo result);

    internal static ListInitiativeSubTypesResponse MapToListInitiativeSubTypesResponse(
        IEnumerable<InitiativeSubTypeEntity> subTypes)
        => new ListInitiativeSubTypesResponse { SubTypes = { MapInitiativeSubTypes(subTypes) } };

    internal static ListInitiativesResponse MapToListInitiativesResponse(IReadOnlyDictionary<DomainEnums.DomainOfInfluenceType, List<DomainModels.Initiative>> groups)
        => new ListInitiativesResponse { Groups = { MapToInitiativeGroups(groups) } };

    internal static ListReferendumDecreesResponse MapToListReferendumDecreesResponse(
        Dictionary<DomainEnums.DomainOfInfluenceType, List<DomainModels.Decree>> decreeGroups)
        => new ListReferendumDecreesResponse { Groups = { MapToDecreeGroups(decreeGroups) } };

    internal static ListDomainOfInfluencesResponse MapToListDomainOfInfluencesResponse(
        IEnumerable<DomainModels.DomainOfInfluence> domainOfInfluences)
        => new ListDomainOfInfluencesResponse
        {
            DomainOfInfluences = { domainOfInfluences.Select(MapToDomainOfInfluence) },
        };

    internal static ListDomainOfInfluenceOwnTypesResponse MapToListDomainOfInfluenceTypesResponse(
        List<DomainEnums.DomainOfInfluenceType> types)
        => new ListDomainOfInfluenceOwnTypesResponse
        {
            DomainOfInfluenceTypes = { MapToDomainOfInfluenceTypes(types) },
        };

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial SharedProtoModels.IdValue MapToIdValue(BaseEntity entity);

    internal static ListCollectionMessagesResponse MapToCollectionMessagesResponse(List<CollectionMessageEntity> messages, bool informalReviewRequested)
        => new ListCollectionMessagesResponse { Messages = { MapToCollectionMessages(messages) }, InformalReviewRequested = informalReviewRequested };

    internal static SearchSignatureSheetPersonCandidatesResponse MapToSearchSignatureSheetPersonsResponse(
        Page<DomainModels.CollectionSignatureSheetCandidate> src)
        => new() { Candidates = { MapSignatureSheetCandidates(src.Items) }, PageInfo = MapToPageInfo(src) };

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapNestedProperties(nameof(CollectionMessageEntity.AuditInfo))]
    internal static partial SharedProtoModels.CollectionMessage MapToCollectionMessage(CollectionMessageEntity message);

    [MapPropertyFromSource(nameof(ProtoAdminModels.Referendum.Collection))]
    internal static partial ProtoAdminModels.Referendum MapToReferendum(DomainModels.Referendum referendum);

    [MapPropertyFromSource(nameof(ProtoAdminModels.Initiative.Collection))]
    internal static partial ProtoAdminModels.Initiative MapToInitiative(DomainModels.Initiative initiative);

    internal static ListCollectionPermissionsResponse MapToListCollectionPermissionsResponse(List<DomainModels.CollectionPermission> collectionPermissions)
        => new ListCollectionPermissionsResponse { Permissions = { MapToCollectionPermissions(collectionPermissions) } };

    internal static partial IEnumerable<DomainOfInfluenceType> MapToDomainOfInfluenceTypes(IEnumerable<DomainEnums.DomainOfInfluenceType> domainOfInfluenceTypes);

    internal static partial IEnumerable<DomainEnums.DomainOfInfluenceType> MapToDomainOfInfluenceTypes(IEnumerable<DomainOfInfluenceType> domainOfInfluenceTypes);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial ProtoAdminModels.InitiativeCommittee MapToInitiativeCommittee(SharedDomainModels.InitiativeCommittee committee);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DomainModels.IVotingStimmregisterPersonInfo.OfficialName), nameof(VerifyInitiativeCommitteeMemberResponse.LastName))]
    [MapProperty(nameof(DomainModels.IVotingStimmregisterPersonInfo.IsVotingAllowed), nameof(VerifyInitiativeCommitteeMemberResponse.HasVotingRight))]
    [MapProperty(nameof(DomainModels.IVotingStimmregisterPersonInfo.ResidenceAddressStreet), nameof(VerifyInitiativeCommitteeMemberResponse.Street))]
    [MapProperty(nameof(DomainModels.IVotingStimmregisterPersonInfo.ResidenceAddressHouseNumber), nameof(VerifyInitiativeCommitteeMemberResponse.HouseNumber))]
    [MapProperty(nameof(DomainModels.IVotingStimmregisterPersonInfo.ResidenceAddressTown), nameof(VerifyInitiativeCommitteeMemberResponse.Locality))]
    [MapProperty(nameof(DomainModels.IVotingStimmregisterPersonInfo.ResidenceAddressZipCode), nameof(VerifyInitiativeCommitteeMemberResponse.ZipCode))]
    internal static partial VerifyInitiativeCommitteeMemberResponse MapToVerifyInitiativeCommitteeMemberResponse(DomainModels.IVotingStimmregisterPersonInfo personInfo);

    internal static partial DomainEnums.CollectionCameNotAboutReason MapToCollectionCameNotAboutReason(CollectionCameNotAboutReason cameNotAboutReason);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial CollectionAddress MapToCollectionAddress(ProtoAdminModels.CollectionAddress source);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static SubmitSignatureSheetsResponse MapToSubmitSignatureSheetsResponse(DomainModels.CollectionUserPermissions userPermissions)
        => new SubmitSignatureSheetsResponse { UserPermissions = MapCollectionUserPermissions(userPermissions) };

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapNestedProperties(nameof(sheet.AuditInfo))]
    [MapNestedProperties(nameof(sheet.CollectionMunicipality))]
    internal static partial ProtoAdminModels.CollectionSignatureSheet MapToCollectionSignatureSheet(DomainModels.CollectionSignatureSheet sheet);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapValue(nameof(DomainModels.VotingStimmregisterPersonFilterData.Bfs), "")]
    [MapValue(nameof(DomainModels.VotingStimmregisterPersonFilterData.ActualityDate), null)]
    internal static partial DomainModels.VotingStimmregisterPersonFilterData MapToPersonFilterData(
        SearchSignatureSheetPersonCandidatesRequest source);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static ListCollectionMunicipalitiesResponse MapToListCollectionMunicipalitiesResponse(List<CollectionMunicipalityEntity> collectionMunicipalities)
        => new ListCollectionMunicipalitiesResponse { Municipalities = { MapToCollectionMunicipalities(collectionMunicipalities) } };

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial SubmitCollectionMunicipalitySignatureSheetsResponse MapToSubmitCollectionMunicipalitySignatureSheetsResponse(DomainModels.SubmitMunicipalitySignatureSheetsResult result);

    internal static ListCollectionMunicipalitySignatureSheetsResponse MapToListCollectionMunicipalitySignatureSheetsResponse(List<DomainModels.CollectionSignatureSheet> signatureSheets)
        => new ListCollectionMunicipalitySignatureSheetsResponse { SignatureSheets = { MapToCollectionSignatureSheets(signatureSheets) } };

    [MapPropertyFromSource(nameof(ProtoAdminModels.DomainOfInfluence.Settings))]
    [MapPropertyFromSource(nameof(ProtoAdminModels.DomainOfInfluence.Address))]
    internal static partial ProtoAdminModels.DomainOfInfluence MapToDomainOfInfluence(DomainModels.DomainOfInfluence domainOfInfluence);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial SubmitSignatureSheetResponse MapToSubmitSignatureSheetResponse(DomainModels.SignatureSheetStateChangeResult result);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial UnsubmitSignatureSheetResponse MapToUnsubmitSignatureSheetResponse(DomainModels.SignatureSheetStateChangeResult result);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DiscardSignatureSheetResponse MapToDiscardSignatureSheetResponse(DomainModels.SignatureSheetStateChangeResult result);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial RestoreSignatureSheetResponse MapToRestoreSignatureSheetResponse(DomainModels.SignatureSheetStateChangeResult result);

    internal static DateOnly? MapToNullableDateOnly(SharedProtoModels.Date? date)
        => date == null ? null : new DateOnly(date.Year, date.Month, date.Day);

    internal static DateOnly MapToDateOnly(SharedProtoModels.Date date)
        => new DateOnly(date.Year, date.Month, date.Day);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial ConfirmSignatureSheetResponse MapToConfirmSignatureSheetResponse(DomainModels.SignatureSheetConfirmResult result);

    internal static ListSignatureSheetSamplesResponse MapToListSignatureSheetSamplesResponse(IEnumerable<CollectionSignatureSheetEntity> sheets)
        => new ListSignatureSheetSamplesResponse { SignatureSheets = { MapToCollectionSignatureSheets(sheets) } };

    internal static AddSignatureSheetSamplesResponse MapToAddSignatureSheetSamplesResponse(IEnumerable<CollectionSignatureSheetEntity> sheets)
        => new AddSignatureSheetSamplesResponse { SignatureSheets = { MapToCollectionSignatureSheets(sheets) } };

    internal static DeleteCollectionImageResponse MapToDeleteCollectionImageResponse(FileEntity? file)
        => new DeleteCollectionImageResponse { GeneratedSignatureSheetTemplate = file == null ? null : MapToFile(file) };

    internal static DeleteCollectionLogoResponse MapToDeleteCollectionLogoResponse(FileEntity? file)
        => new DeleteCollectionLogoResponse { GeneratedSignatureSheetTemplate = file == null ? null : MapToFile(file) };

    internal static DeleteSignatureSheetTemplateResponse MapToDeleteSignatureSheetTemplateResponse(FileEntity file)
        => new DeleteSignatureSheetTemplateResponse { GeneratedSignatureSheetTemplate = MapToFile(file) };

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ReferendumDeleteInfo MapToReferendumDeleteInfo(DomainModels.ReferendumDeleteInfo info);

    private static DateOnly MapToDateOnly(Timestamp timestamp)
        => DateOnly.FromDateTime(MapToDateTime(timestamp));

    private static partial IEnumerable<ProtoAdminModels.CollectionsGroup> MapToCollectionsGroup(
        IReadOnlyDictionary<DomainEnums.DomainOfInfluenceType, DomainModels.CollectionsGroup> groups);

    [MapProperty(
        nameof(KeyValuePair<DomainOfInfluenceType, DomainModels.CollectionsGroup>.Key),
        nameof(ProtoAdminModels.CollectionsGroup.DomainOfInfluenceType))]
    [MapNestedProperties(nameof(KeyValuePair<DomainEnums.DomainOfInfluenceType, DomainModels.CollectionsGroup>.Value))]
    private static partial ProtoAdminModels.CollectionsGroup MapToCollectionsGroup(
        KeyValuePair<DomainEnums.DomainOfInfluenceType, DomainModels.CollectionsGroup> groups);

    private static partial IEnumerable<ProtoAdminModels.CollectionSignatureSheet> MapToCollectionSignatureSheets(IEnumerable<CollectionSignatureSheetEntity> sheets);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapNestedProperties(nameof(sheet.AuditInfo))]
    [MapNestedProperties(nameof(sheet.CollectionMunicipality))]
    [MapperIgnoreTarget(nameof(ProtoAdminModels.CollectionSignatureSheet.UserPermissions))]
    private static partial ProtoAdminModels.CollectionSignatureSheet MapToCollectionSignatureSheet(CollectionSignatureSheetEntity sheet);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.CollectionAddress MapToCollectionAddress(CollectionAddress source);

    private static partial IEnumerable<ProtoAdminModels.Initiative> MapToInitiatives(IEnumerable<DomainModels.Initiative> initiativeSubTypes);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(@InitiativeCommitteeMemberEntity.Permission.Role), nameof(ProtoAdminModels.InitiativeCommitteeMember.Role))]
    private static partial ProtoAdminModels.InitiativeCommitteeMember MapToInitiativeCommitteeMember(SharedDomainModels.InitiativeCommitteeMember member);

    private static partial IEnumerable<ProtoAdminModels.InitiativeSubType> MapInitiativeSubTypes(IEnumerable<InitiativeSubTypeEntity> subTypes);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial SharedProtoModels.File MapToFile(FileEntity file);

    private static partial IEnumerable<ProtoAdminModels.Certificate> MapToCertificates(IEnumerable<CertificateEntity> certs);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(CertificateEntity.CAInfo), nameof(ProtoAdminModels.Certificate.CaInfo))]
    [MapProperty(nameof(@CertificateEntity.AuditInfo.CreatedByName), nameof(ProtoAdminModels.Certificate.ImportedByName))]
    [MapProperty(nameof(@CertificateEntity.AuditInfo.CreatedAt), nameof(ProtoAdminModels.Certificate.ImportedAt))]
    private static partial ProtoAdminModels.Certificate MapToCertificate(CertificateEntity cert);

    private static partial IEnumerable<ProtoAdminModels.CollectionPermission> MapToCollectionPermissions(IEnumerable<DomainModels.CollectionPermission> collectionPermissions);

    private static partial ProtoAdminModels.CollectionPermission MapToCollectionPermission(DomainModels.CollectionPermission collectionPermission);

    private static partial IEnumerable<SharedProtoModels.CollectionMessage> MapToCollectionMessages(List<CollectionMessageEntity> messages);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.CollectionSignatureSheetCount MapToCollectionSignatureSheetCount(CollectionSignatureSheetCount sheet);

    private static partial IEnumerable<ProtoAdminModels.Decree> MapToDecrees(IEnumerable<DomainModels.Decree> decree);

    private static partial IEnumerable<ProtoAdminModels.CollectionSignatureSheetCandidate> MapSignatureSheetCandidates(IEnumerable<DomainModels.CollectionSignatureSheetCandidate> src);

    [UserMapping(Default = true)]
    private static ProtoAdminModels.CollectionSignatureSheetCandidate MapSignatureSheetCandidate(
        DomainModels.CollectionSignatureSheetCandidate src)
    {
        var mapped = MapSignatureSheetCandidateInternal(src);
        if (mapped.ExistingSignature != null)
        {
            mapped.ExistingSignature.IsInSameMunicipality = src.ExistingSignatureIsInSameMunicipality;
            mapped.ExistingSignature.IsOnSameSheet = src.ExistingSignatureIsOnSameSheet;
        }

        return mapped;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.CollectionSignatureSheetCandidate MapSignatureSheetCandidateInternal(DomainModels.CollectionSignatureSheetCandidate src);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(ProtoAdminModels.CollectionSignatureSheetCandidateExistingSignature.IsInSameMunicipality))]
    [MapperIgnoreTarget(nameof(ProtoAdminModels.CollectionSignatureSheetCandidateExistingSignature.IsOnSameSheet))]
    [MapNestedProperties(nameof(src.CollectionMunicipality))]
    [MapProperty([nameof(src.CollectionMunicipality), nameof(src.CollectionMunicipality.Collection), nameof(src.CollectionMunicipality.Collection.Description)], nameof(ProtoAdminModels.CollectionSignatureSheetCandidateExistingSignature.CollectionDescription))]
    private static partial ProtoAdminModels.CollectionSignatureSheetCandidateExistingSignature MapSignatureSheetCandidateExistingSignature(CollectionCitizenEntity src);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.Person MapPerson(DomainModels.IVotingStimmregisterPersonInfo src);

    [MapperIgnoreSource(nameof(DomainModels.Decree.AuditInfo))]
    [MapperIgnoreSource(nameof(DomainModels.Decree.Collections))]
    [MapperIgnoreSource(nameof(DomainModels.Decree.IntegritySignatureInfo))]
    [MapperIgnoreSource(nameof(DomainModels.Decree.CollectionCitizenLogs))]
    [MapProperty(nameof(DomainModels.Decree.Referendums), nameof(ProtoAdminModels.Decree.Collections))]
    private static partial ProtoAdminModels.Decree MapToDecree(DomainModels.Decree decree);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(ProtoAdminModels.Collection.UserPermissions))]
    [MapperIgnoreTarget(nameof(ProtoAdminModels.Collection.AttestedCollectionCount))]
    private static partial ProtoAdminModels.Collection MapToInitiativeCollection(InitiativeEntity collectionEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.Collection MapToInitiativeCollection(DomainModels.Initiative initiative);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(ProtoAdminModels.Collection.UserPermissions))]
    [MapperIgnoreTarget(nameof(ProtoAdminModels.Collection.AttestedCollectionCount))]
    private static partial ProtoAdminModels.Collection MapToReferendumCollection(ReferendumEntity collectionEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.Collection MapToReferendumCollection(DomainModels.Referendum referendum);

    [MapPropertyFromSource(nameof(ProtoAdminModels.Initiative.Collection))]
    [MapperIgnoreTarget(nameof(ProtoAdminModels.Initiative.DomainOfInfluenceName))]
    private static partial ProtoAdminModels.Initiative MapToInitiative(InitiativeEntity initiativeEntity);

    [MapPropertyFromSource(nameof(ProtoAdminModels.Referendum.Collection))]
    private static partial ProtoAdminModels.Referendum MapToReferendum(ReferendumEntity referendumEntity);

    private static partial DomainEnums.DomainOfInfluenceType MapToDomainOfInfluenceType(DomainOfInfluenceType domainOfInfluenceType);

    private static partial ProtoAdminModels.InitiativeSubType MapToInitiativeSubType(InitiativeSubTypeEntity initiativeSubTypeEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.CollectionCount MapToCollectionCount(CollectionCountEntity collectionCountEntity);

    private static partial IEnumerable<ProtoAdminModels.DecreeGroup> MapToDecreeGroups(
        IReadOnlyDictionary<DomainEnums.DomainOfInfluenceType, List<DomainModels.Decree>> groups);

    private static partial IEnumerable<ProtoAdminModels.InitiativeGroup> MapToInitiativeGroups(
        IReadOnlyDictionary<DomainEnums.DomainOfInfluenceType, List<DomainModels.Initiative>> groups);

    [MapProperty(
        nameof(KeyValuePair<DomainEnums.DomainOfInfluenceType, List<DomainModels.Decree>>.Key),
        nameof(ProtoAdminModels.DecreeGroup.DomainOfInfluenceType))]
    [MapProperty(
        nameof(KeyValuePair<DomainEnums.DomainOfInfluenceType, List<DomainModels.Decree>>.Value),
        nameof(ProtoAdminModels.DecreeGroup.Decrees))]
    private static partial ProtoAdminModels.DecreeGroup MapToDecreeGroup(KeyValuePair<DomainEnums.DomainOfInfluenceType, List<DomainModels.Decree>> groups);

    [MapProperty(
        nameof(KeyValuePair<DomainEnums.DomainOfInfluenceType, List<DomainModels.Initiative>>.Key),
        nameof(ProtoAdminModels.InitiativeGroup.DomainOfInfluenceType))]
    [MapProperty(
        nameof(KeyValuePair<DomainEnums.DomainOfInfluenceType, List<DomainModels.Initiative>>.Value),
        nameof(ProtoAdminModels.InitiativeGroup.Initiatives))]
    private static partial ProtoAdminModels.InitiativeGroup MapToInitiativeGroup(KeyValuePair<DomainEnums.DomainOfInfluenceType, List<DomainModels.Initiative>> groups);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.CollectionUserPermissions MapCollectionUserPermissions(DomainModels.CollectionUserPermissions source);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.DecreeUserPermissions MapDecreeUserPermissions(DomainModels.DecreeUserPermissions source);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.CollectionMunicipality MapCollectionMunicipality(CollectionMunicipalityEntity source);

    private static partial IEnumerable<ProtoAdminModels.CollectionMunicipality> MapToCollectionMunicipalities(List<CollectionMunicipalityEntity> municipalities);

    private static partial SharedProtoModels.PageInfo MapToPageInfo(PageInfo sheets);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.SimpleDecree MapToSimpleDecree(DecreeEntity decree);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.InitiativeCommitteeMemberUserPermissions MapInitiativeCommitteeMemberUserPermissions(SharedDomainModels.InitiativeCommitteeMemberUserPermissions userPermissions);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DomainModels.DomainOfInfluence.AddressName), nameof(ProtoAdminModels.DomainOfInfluenceAddress.Name))]
    private static partial ProtoAdminModels.DomainOfInfluenceAddress MapToDomainOfInfluenceAddress(DomainModels.DomainOfInfluence domainOfInfluence);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoAdminModels.DomainOfInfluenceSettings MapToDomainOfInfluenceSettings(DomainModels.DomainOfInfluence domainOfInfluence);

    private static DomainModels.DomainOfInfluenceSettings MapDomainOfInfluenceSettings(
        UpdateDomainOfInfluenceSettings settings)
    {
        return new DomainModels.DomainOfInfluenceSettings
        {
            InitiativeMinSignatureCount = settings.HasInitiativeMinSignatureCount
                ? settings.InitiativeMinSignatureCount
                : null,
            ReferendumMaxElectronicSignaturePercent = settings.HasReferendumMaxElectronicSignaturePercent
                ? settings.ReferendumMaxElectronicSignaturePercent
                : null,
            ReferendumMinSignatureCount = settings.HasReferendumMinSignatureCount
                ? settings.ReferendumMinSignatureCount
                : null,
            InitiativeMaxElectronicSignaturePercent = settings.HasInitiativeMaxElectronicSignaturePercent
                ? settings.InitiativeMaxElectronicSignaturePercent
                : null,
            InitiativeNumberOfMembersCommittee = settings.HasInitiativeNumberOfMembersCommittee
                ? settings.InitiativeNumberOfMembersCommittee
                : null,
        };
    }

    private static DateTime MapToDateTime(Timestamp timestamp)
    {
        return timestamp.ToDateTime();
    }

    private static partial IEnumerable<Timestamp> MapToTimestamps(IEnumerable<DateTime> dateTime);

    private static Timestamp MapToTimestamp(DateTime dateTime)
    {
        return Timestamp.FromDateTime(dateTime);
    }

    private static Timestamp MapToTimestamp(DateOnly date)
        => date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToTimestamp();

    private static string? MapToString(string? v)
    {
        // proto does not support null for strings.
        return v ?? string.Empty;
    }

    private static string? MapToString(Guid? v)
    {
        // proto does not support null for strings.
        return v.ToString();
    }

    private static Guid? MapToGuid(string? v)
    {
        // proto does not support null for strings, but sends empty string
        return string.IsNullOrEmpty(v)
            ? null
            : Guid.Parse(v);
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial SharedProtoModels.Date MapToDate(DateOnly date);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial SharedProtoModels.MarkdownString MapMarkdownString(MarkdownString mdStr);
}
